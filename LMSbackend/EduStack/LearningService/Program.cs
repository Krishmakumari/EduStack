// Program.cs — Composition Root for the Learning Service.
// • Registers: LearningDbContext, ProgressRepository, LearningService.
// • UNIQUE: Registers typed HttpClient for EnrollmentClient (cross-service call).
// • Validates JWT tokens issued by Auth Service (same Jwt:Key — symmetric trust).
// • Pipeline: ExceptionMiddleware → Swagger → CORS → Auth → Controllers.

using System.Text;
using LearningService.API.Middleware;
using LearningService.Application.Interfaces;
using LearningService.Infrastructure.External;
using LearningService.Infrastructure.Persistence;
using LearningService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ──────────────────────────────────────────────────────────────
// Separate database for Learning Service (database-per-service pattern).
// Only stores LessonProgress records — no enrollment or course data.
builder.Services.AddDbContext<LearningDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LearningConnection")));

// ─── Repositories & Services ───────────────────────────────────────────────
// Scoped = one per request, matching DbContext lifetime.
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
// Fully qualified name — namespace and class both named "LearningService".
builder.Services.AddScoped<ILearningService, LearningService.Application.Services.LearningService>();

// ─── HTTP Client (EnrollmentService) ──────────────────────────────────────
// Typed HttpClient registration — used to call Enrollment Service for enrollment checks.
// AddHttpClient<TInterface, TImplementation>() uses IHttpClientFactory internally:
//   • Manages connection pooling (no socket exhaustion)
//   • BaseAddress configured here — not hardcoded in EnrollmentClient class
//   • In production: chain .AddPolicyHandler(retryPolicy) for Polly resilience
builder.Services.AddHttpClient<IEnrollmentClient, EnrollmentClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:EnrollmentService"]!);
});

// ─── JWT Authentication ────────────────────────────────────────────────────
// This service does NOT issue tokens — Auth Service does.
// Same Jwt:Key as Auth Service — symmetric key trust model.
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,       // reject expired tokens
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero      // no grace period — expire exactly on time
    };
});

builder.Services.AddAuthorization();

// ─── Controllers ───────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Swagger ───────────────────────────────────────────────────────────────
// Bearer token definition for the Swagger UI "Authorize" button.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EduStack — Learning Service",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token from AuthService login."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ─── Logging ───────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// ─── Middleware Pipeline ───────────────────────────────────────────────────
// ORDER MATTERS:
// 1. DeveloperExceptionPage — detailed errors in development
// 2. GlobalExceptionMiddleware — maps domain exceptions to clean JSON
// 3. Swagger UI — API documentation
// 4. CORS — allow cross-origin requests
// 5. Authentication — reads JWT, populates User claims
// 6. Authorization — enforces [Authorize] attributes
// 7. MapControllers — routes to endpoints

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseMiddleware<GlobalExceptionMiddleware>();  // must be early to catch all errors

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LearningService v1");
    options.RoutePrefix = string.Empty;  // Swagger UI at root URL
});

app.UseCors("AllowAll");
app.UseAuthentication();   // must come BEFORE Authorization
app.UseAuthorization();
app.MapControllers();

app.Run();

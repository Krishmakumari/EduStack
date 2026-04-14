// Program.cs — Composition Root for the Enrollment Service.
// • Registers DI: EnrollmentDbContext, repositories, EnrollmentService.
// • Validates JWT tokens issued by Auth Service (same Jwt:Key — symmetric trust).
// • Pipeline: ExceptionMiddleware → Swagger → CORS → Auth → Controllers.

using System.Text;
using EnrollmentService.API.Middleware;
using EnrollmentService.Application.Interfaces;
using EnrollmentService.Application.Services;
using EnrollmentService.Infrastructure.Persistence;
using EnrollmentService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ──────────────────────────────────────────────────────────────
// Database-per-service pattern. Enrollment Service has its own tables
// (Enrollments, LessonProgresses) separate from Auth and Course services.
builder.Services.AddDbContext<EnrollmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Repositories ──────────────────────────────────────────────────────────
// Scoped = one instance per HTTP request (matches EF DbContext lifetime).
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ILessonProgressRepository, LessonProgressRepository>();

// ─── Services ──────────────────────────────────────────────────────────────
// Fully qualified name needed — namespace and class both called "EnrollmentService".
builder.Services.AddScoped<IEnrollmentService,
    EnrollmentService.Application.Services.EnrollmentService>();

// ─── JWT Authentication ────────────────────────────────────────────────────
// This service does NOT issue tokens — Auth Service does.
// It only VALIDATES tokens to know who the student/instructor is.
// IMPORTANT: Must use the SAME Jwt:Key as Auth Service (symmetric key).
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
        IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero      // no grace period — expired = rejected immediately
    };
});

builder.Services.AddAuthorization();

// ─── Controllers ───────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Swagger ───────────────────────────────────────────────────────────────
// Bearer token security definition so Swagger UI has the "Authorize" button.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EduStack — Enrollment Service",
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
// Allow all origins in development. Tighten in production.
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

// ─── Middleware Pipeline ────────────────────────────────────────────────────
// ORDER MATTERS:
// 1. DeveloperExceptionPage — detailed errors in development only.
// 2. GlobalExceptionMiddleware — maps domain exceptions to JSON error responses.
// 3. Swagger — API documentation UI.
// 4. CORS — allow cross-origin requests.
// 5. Authentication — reads JWT, populates User claims.
// 6. Authorization — enforces [Authorize(Roles = ...)] attributes.
// 7. MapControllers — routes to endpoint methods.

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseMiddleware<GlobalExceptionMiddleware>();  // must be early to catch all errors

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "EnrollmentService v1");
    options.RoutePrefix = string.Empty;   // Swagger UI at root URL
});

app.UseCors("AllowAll");
app.UseAuthentication();   // must come BEFORE Authorization
app.UseAuthorization();
app.MapControllers();

app.Run();
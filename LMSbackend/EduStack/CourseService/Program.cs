// Program.cs — Composition Root for the Course Service.
// • Registers DI: CourseDbContext, repositories, CourseService.
// • Validates JWT tokens issued by Auth Service (same Jwt:Key).
// • Pipeline: ExceptionMiddleware → Swagger → CORS → Auth → Controllers.

using System.Text;
using CourseService.API.Middleware;
using CourseService.Application.Interfaces;
using CourseService.Application.Services;
using CourseService.Infrastructure.Persistence;
using CourseService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ──────────────────────────────────────────────────────────────
// Each microservice has its own database (database-per-service pattern).
// CourseConnection is separate from Auth Service's DefaultConnection.
builder.Services.AddDbContext<CourseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CourseConnection")));

// ─── Repositories ──────────────────────────────────────────────────────────
// Scoped = one instance per HTTP request (matches DbContext lifetime).
// Three repositories for the three entities: Course, Section, Lesson.
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();

// ─── Services ──────────────────────────────────────────────────────────────
// Single service handles all course/section/lesson business logic.
// Fully qualified name needed because namespace and class share the name "CourseService".
builder.Services.AddScoped<ICourseService, CourseService.Application.Services.CourseService>();

// ─── JWT Authentication ────────────────────────────────────────────────────
// This service does NOT issue tokens — Auth Service does.
// It only VALIDATES tokens to know who the user is and what role they have.
// IMPORTANT: Must use the SAME Jwt:Key as Auth Service (symmetric key trust).
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
        ClockSkew = TimeSpan.Zero      // no grace period — expire exactly on time
    };
});

builder.Services.AddAuthorization();

// ─── Controllers ───────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Swagger ───────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EduStack — Course Service",
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

// ══════════════════════════════════════════════════════════════════════════
var app = builder.Build();
// ══════════════════════════════════════════════════════════════════════════

// MIDDLEWARE PIPELINE ORDER MATTERS:
// 1. DeveloperExceptionPage — detailed errors in development.
// 2. GlobalExceptionMiddleware — catches domain exceptions, returns clean JSON.
// 3. Swagger — API documentation UI.
// 4. CORS — allow cross-origin requests from frontend.
// 5. Authentication — reads JWT from header, populates User claims.
// 6. Authorization — checks [Authorize(Roles = ...)] attributes.
// 7. MapControllers — routes requests to controller endpoints.

app.UseDeveloperExceptionPage();
app.UseMiddleware<GlobalExceptionMiddleware>();  // must be early to catch all errors

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CourseService v1");
    options.RoutePrefix = string.Empty;   // Swagger UI at root URL
});

app.UseCors("AllowAll");
app.UseAuthentication();   // must come BEFORE Authorization
app.UseAuthorization();
app.MapControllers();

app.Run();
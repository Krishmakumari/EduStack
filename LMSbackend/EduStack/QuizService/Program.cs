// Program.cs — Composition Root for the Quiz Service.
// • Sets up QuizDbContext and simple DI (IQuizService).
// • Architectural Note: QuizService intentionally omits the Repository pattern, instead using DbContext directly.
// • Uses "YourSuperSecretKey" for JWT symmetric signature matching the Auth Service.
// • Swagger config includes Bearer token functionality.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuizService.API.Middleware;
using QuizService.Application.Interfaces;
using QuizService.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QuizConnection")));
Console.WriteLine(builder.Configuration.GetConnectionString("QuizConnection")); // debug logic

// ─── DI ───────────────────────────────────────────────────────────────────
// Directly binding the Service (no IRepository interface used in this microservice).
builder.Services.AddScoped<IQuizService, QuizService.Application.Services.QuizService>();

// ─── JWT Authentication ───────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            // 🔥 Shared secret with Auth Service
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("YourSuperSecretKey"))
        };
    });

// ─── Controllers ──────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Swagger + JWT Support ────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT support to Swagger UI.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ──────────────────────────────────────────────────

// 1. Catches and translates domain errors -> 400/403/404 HTTP codes
app.UseMiddleware<ExceptionMiddleware>();

// 2. Interactive documentation
app.UseSwagger();
app.UseSwaggerUI();

// 3. Security Check layers
app.UseAuthentication();
app.UseAuthorization();

// 4. API Endpoints
app.MapControllers();

app.Run();
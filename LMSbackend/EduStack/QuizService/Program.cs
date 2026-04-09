using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuizService.API.Middleware;
using QuizService.Application.Interfaces;
using QuizService.Application.Services;
using QuizService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);


// ─────────────────────────────────────────────────────────────
//  Database
// ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ─────────────────────────────────────────────────────────────
//  Dependency Injection
// ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IQuizService, QuizService.Application.Services.QuizService>();


// ─────────────────────────────────────────────────────────────
//  JWT Authentication
// ─────────────────────────────────────────────────────────────
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

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("YourSuperSecretKey")) // 🔥 same as AuthService
        };
    });


// ─────────────────────────────────────────────────────────────
//  Controllers
// ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();


// ─────────────────────────────────────────────────────────────
//  Swagger + JWT Support
// ─────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //  Add JWT to Swagger
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


// ─────────────────────────────────────────────────────────────
//  Global Exception Middleware
// ─────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();


// ─────────────────────────────────────────────────────────────
//  Swagger UI
// ─────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI();


// ─────────────────────────────────────────────────────────────
//  Auth Middleware
// ─────────────────────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();


// ─────────────────────────────────────────────────────────────
//  Map Controllers
// ─────────────────────────────────────────────────────────────
app.MapControllers();

app.Run();
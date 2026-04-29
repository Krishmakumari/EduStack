// Program.cs — Composition Root for the Payment Service.
// • Registers: PaymentDbContext, repositories (Scoped), PaymentService (fully-qualified name).
// • JWT validation using the same symmetric key as Auth Service.
// • Swagger with Bearer token security definition for the Authorize button.
// • Pipeline: ExceptionMiddleware → CORS → Swagger → Auth → Controllers.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentService.API.Middleware;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ──────────────────────────────────────────────────────────────
// Database-per-service. Payment Service owns Payments + Refunds tables.
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentConnection")));

// ─── DI ───────────────────────────────────────────────────────────────────
// Scoped = one instance per HTTP request (matches DbContext lifetime).
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();

// Fully qualified name required: namespace and class both called "PaymentService".
builder.Services.AddScoped<IPaymentService, PaymentService.Application.Services.PaymentService>();

// ─── Controllers ───────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Swagger ───────────────────────────────────────────────────────────────
// Bearer token security definition enables the "Authorize" button in Swagger UI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EduStack - Payment Service",
        Version = "v1"
    });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── JWT Authentication ────────────────────────────────────────────────────
// Validates tokens issued by Auth Service using the SAME symmetric Jwt:Key.
// Payment Service does NOT issue tokens — it only validates them.
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,          // reject expired tokens

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!))  // same key as Auth Service
        };
    });

builder.Services.AddAuthorization();

// ─── CORS ─────────────────────────────────────────────────────────────────
// Allow Angular dev server and production origin to call this service.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ─── App Pipeline ──────────────────────────────────────────────────────────
// ORDER MATTERS:
// 1. ExceptionMiddleware — catches all errors before they reach auth/controllers
// 2. Swagger — API documentation
// 3. Authentication — validate JWT, populate User claims
// 4. Authorization — enforce [Authorize] and [Authorize(Roles="...")] attributes
// 5. MapControllers — route to endpoints

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();  // must be first to catch all errors

app.UseCors();  // must be before auth and controllers

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService v1");
    options.RoutePrefix = string.Empty;   // Swagger at root URL
});

app.UseAuthentication();   // must come before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
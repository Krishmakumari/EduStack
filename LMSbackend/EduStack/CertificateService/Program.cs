// Program.cs — Composition Root for the Certificate Service.
// • Registers DI: CertificateDbContext, CertificateService, PdfGenerator.
// • No JWT authentication — simpler service (add in production).
// • No GlobalExceptionMiddleware — exceptions return as 500 by default.
// • Pipeline: Swagger (dev only) → HTTPS Redirect → Auth → Controllers.

using CertificateService.Application.Interfaces;
using CertificateService.Application.Services;
using CertificateService.Infrastructure.Persistence;
using CertificateService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── QuestPDF License ──────────────────────────────────────────────────────
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ─── Database ──────────────────────────────────────────────────────────────
// Uses DefaultConnection — same DB server, separate database for this service.
builder.Services.AddDbContext<CertificateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── JWT Authentication ────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ─── Services ──────────────────────────────────────────────────────────────
// CertificateService — business logic (generate + download).
// Fully qualified name needed because namespace and class both use "CertificateService".
builder.Services.AddScoped<ICertificateService, CertificateService.Application.Services.CertificateService>();

// PdfGenerator — QuestPDF-based PDF creation. Stateless, so Scoped is fine.
builder.Services.AddScoped<PdfGenerator>();

// RabbitMqPublisher — Event publishing logic.
builder.Services.AddScoped<CertificateService.Infrastructure.Messaging.RabbitMqPublisher>();

// ─── Controllers ───────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Swagger ───────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─── Middleware Pipeline ───────────────────────────────────────────────────
// Swagger only in development — hide API docs in production.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();   // redirect HTTP → HTTPS for security

app.UseAuthentication();
app.UseAuthorization();      // no JWT auth configured yet — placeholder for future

app.MapControllers();

app.Run();
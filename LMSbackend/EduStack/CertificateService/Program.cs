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

// ─── Database ──────────────────────────────────────────────────────────────
// Uses DefaultConnection — same DB server, separate database for this service.
builder.Services.AddDbContext<CertificateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Services ──────────────────────────────────────────────────────────────
// CertificateService — business logic (generate + download).
// Fully qualified name needed because namespace and class both use "CertificateService".
builder.Services.AddScoped<ICertificateService, CertificateService.Application.Services.CertificateService>();

// PdfGenerator — QuestPDF-based PDF creation. Stateless, so Scoped is fine.
builder.Services.AddScoped<PdfGenerator>();

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

app.UseAuthorization();      // no JWT auth configured yet — placeholder for future

app.MapControllers();

app.Run();
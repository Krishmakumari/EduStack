using CertificateService.Application.Interfaces;
using CertificateService.Application.Services;
using CertificateService.Infrastructure.Persistence;
using CertificateService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ DB
builder.Services.AddDbContext<CertificateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Services
builder.Services.AddScoped<ICertificateService, CertificateService.Application.Services.CertificateService>();
builder.Services.AddScoped<PdfGenerator>();

// ✅ Controllers
builder.Services.AddControllers();

// ✅ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
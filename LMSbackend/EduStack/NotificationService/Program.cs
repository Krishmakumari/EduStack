// Program.cs — Composition Root for the Notification Service.
// • No JWT auth — notification sending is internal (called by other services or manually).
// • EmailService and RabbitMqConnection are Singleton — stateless / expensive to create.
// • RabbitMqConsumer registered as IHostedService — starts at app startup, runs indefinitely.
// • Two notification paths: HTTP (NotificationController) + async (RabbitMqConsumer).

using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Email Service ─────────────────────────────────────────────────────────
// Singleton: stateless, reads config once, thread-safe. No need for a new instance per request.
builder.Services.AddSingleton<EmailService>();

// ─── RabbitMQ Connection ───────────────────────────────────────────────────
// Singleton: TCP connection to RabbitMQ broker is expensive to create.
// One connection shared for the entire app lifetime — used by RabbitMqConsumer.
builder.Services.AddSingleton<RabbitMqConnection>();

// ─── Application Service ───────────────────────────────────────────────────
// Scoped: standard lifetime for application services called via HTTP requests.
builder.Services.AddScoped<INotificationService, NotificationAppService>();

// ─── Background Consumer ───────────────────────────────────────────────────
// IHostedService: automatically started when the app starts, stopped when it stops.
// Listens to certificate_queue, enrollment_queue, quiz_queue indefinitely.
builder.Services.AddHostedService<RabbitMqConsumer>();

// ─── Controllers + Swagger ────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─── Middleware Pipeline ───────────────────────────────────────────────────
// Swagger only in development (no bearer token needed — service is internal).
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();   // placeholder — no [Authorize] attributes currently
app.MapControllers();

app.Run();

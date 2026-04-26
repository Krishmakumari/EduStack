// APIGateway Program.cs — The single entry point for all frontend/client requests.
// • Acts as a reverse proxy routing requests to the appropriate backend microservice.
// • Aggregates Swagger documentation from all 8 downstream microservices into one UI.
// • Performs edge-level JWT Authentication before forwarding requests.
// • Handles CORS policies centrally for the entire EduStack ecosystem.

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ─── Ocelot Configuration ──────────────────────────────────────────────────
// Load the routing mapping rules from ocelot.json.
// reloadOnChange: true allows changing routes without restarting the gateway.
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ─── Edge JWT Authentication ───────────────────────────────────────────────
// Validates JWTs at the gateway level. If a token is invalid/expired, 
// the request is rejected with 401 Unauthorized before it even reaches a microservice.
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
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero  // enforce strict token expiration times
    };
});

// ─── CORS Policy ───────────────────────────────────────────────────────────
// Centralized CORS management. Since the frontend only talks to the API Gateway,
// we only need to define CORS here, not in every individual microservice.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ─── Ocelot & Swagger Services ─────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOcelot();

// MMLib.SwaggerForOcelot enables fetching swagger.json from downstream services
// and merging them into a unified Swagger UI dropdown.
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// ─── Logging ───────────────────────────────────────────────────────────────
// Restrict logging to Console for clean Docker/Console output
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// 1. Apply CORS globally
app.UseCors("AllowAll");

// 2. Swagger UI Pipeline
app.UseSwaggerForOcelotUI(opt =>
{
    // The path where the unified Swagger UI will be hosted
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// 3. Authentication & Authorization Pipeline
app.UseAuthentication();
app.UseAuthorization();

// 4. Ocelot Reverse Proxy Pipeline
// Must be awaited. Ocelot takes over the request lifecycle from here and routes it downwards.
await app.UseOcelot();

app.Run();

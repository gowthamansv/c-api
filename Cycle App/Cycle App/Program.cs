using System.Text;
using Cycle_App.Data;
using Cycle_App.Services.WhatsApp;
using CycleMaintenance.Services.Authentication;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// FastEndpoints + Swagger
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument();

// Database
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Application Services
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddScoped<IAccountActivationService, AccountActivationService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are not configured.");

// JWT Authentication
// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateLifetime = true,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),

                ValidateIssuerSigningKey = true
            };
    });

// Authorization
builder.Services.AddAuthorization();
// Authorizationbuilder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.UseFastEndpoints();

app.UseSwaggerGen();

app.Run();
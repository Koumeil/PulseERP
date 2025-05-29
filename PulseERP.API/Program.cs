using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PulseERP.API.ErrorHandling;
using PulseERP.Application;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Settings;
using PulseERP.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// === Serilog Configuration ===
builder.Host.UseSerilog(
    (ctx, lc) =>
        lc
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .ReadFrom.Configuration(ctx.Configuration)
);

// === Services configuration ===
builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();

// Personnalisation des ProblemDetails pour la validation
builder.Services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
    options.InvalidModelStateResponseFactory = context =>
    {
        var factory =
            context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var validationDetails = factory.CreateValidationProblemDetails(
            context.HttpContext,
            context.ModelState,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Model validation failed",
            detail: "See the errors property for details."
        );
        return new ObjectResult(validationDetails) { StatusCode = validationDetails.Status };
    };
});

// Email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// SmtpEmailService
builder.Services.AddTransient<ISmtpEmailService, SmtpEmailService>();

// Authentication (JWT Bearer)
var jwtSettings =
    builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings missing");

builder
    .Services.AddAuthentication(options =>
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
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
            ),

            ClockSkew = TimeSpan.Zero,
        };
    });

// Application + Infrastructure DI
builder.Services.AddApplication().AddInfrastructure(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// === Swagger / OpenAPI configuration ===
builder.Services.AddSwaggerGen(c =>
{
    // Définition du schéma Bearer pour JWT
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Entrez 'Bearer {votre_token}'",
        }
    );

    // Exigence de ce schéma pour tous les endpoints protégés
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

Log.Information("Starting application...");

// === Build & Middleware pipeline ===
var app = builder.Build();

// Global exception handling
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PulseERP API V1");
        // c.RoutePrefix = string.Empty; // décommentez pour Swagger à la racine
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
Log.CloseAndFlush();

using Hellang.Middleware.ProblemDetails;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Abstractions.Settings;
using PulseERP.API.Extensions;
using PulseERP.Application;
using PulseERP.Infrastructure;
using PulseERP.Infrastructure.Smtp;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ────────────────────────────────────────────────────────────────
builder.Host.UseSerilog(
    (ctx, lc) =>
        lc
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "PulseERP")
            .Enrich.WithMachineName()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
);

// Secrets:

builder.Configuration.AddUserSecrets<Program>(optional: true);
builder.Configuration.AddEnvironmentVariables();

// Memory Cache:
builder.Services.AddMemoryCache();

// Redis

var redisConfig = builder.Configuration["RedisSettings:Configuration"];
var redisInstance = builder.Configuration["RedisSettings:InstanceName"];

// 2. Vérifier qu’on n’est pas en null
if (string.IsNullOrWhiteSpace(redisConfig))
{
    throw new InvalidOperationException(
        "RedisSettings:Configuration est null ou vide. Veuillez le configurer dans appsettings ou via une variable d’environnement."
    );
}

// 2) Enregistrement du cache Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig;
    options.InstanceName = redisInstance;
});

// ─── Services ───────────────────────────────────────────────────────────────
builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddTransient<IEmailSenderService, SmtpEmailService>();
builder.Services.AddApplication().AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// ─── Swagger / OpenAPI ─────────────────────────────────────────────────────
// Swagger extracted
builder.Services.AddCustomSwagger();

// ─── JWT Authentication ─────────────────────────────────────────────────────
var jwtSettings =
    builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings missing");

builder.Services.AddJwtWithChallenge(jwtSettings);

// ─── ProblemDetails (Hellang) ──────────────────────────────────────────────
builder.Services.AddCustomProblemDetails(jwtSettings, builder.Environment);

var app = builder.Build();

// ─── Middleware pipeline ───────────────────────────────────────────────────
app.UseProblemDetails();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PulseERP API V1");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

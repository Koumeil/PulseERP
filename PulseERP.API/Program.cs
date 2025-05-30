using Hellang.Middleware.ProblemDetails;
using PulseERP.API.Extensions;
using PulseERP.Application;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Settings;
using PulseERP.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ────────────────────────────────────────────────────────────────
builder.Host.UseSerilog(
    (ctx, lc) =>
        lc
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .ReadFrom.Configuration(ctx.Configuration)
);

// ─── Services ───────────────────────────────────────────────────────────────
builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddTransient<ISmtpEmailService, SmtpEmailService>();
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

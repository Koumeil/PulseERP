using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using PulseERP.API.ErrorHandling;
using PulseERP.Application;
using PulseERP.Application.Settings;
using PulseERP.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog(
    (ctx, lc) =>
        lc
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .ReadFrom.Configuration(ctx.Configuration)
);

builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Empêche ASP.NET Core de renvoyer automatiquement une réponse 400 quand la validation échoue.
    // Cela te permet d'intercepter ces erreurs et de les formater à ta façon (ex: avec ExtendedValidationProblemDetails).
    options.SuppressModelStateInvalidFilter = true;

    // Personnalise la réponse quand la validation du modèle échoue
    options.InvalidModelStateResponseFactory = context =>
    {
        // Crée un ValidationProblemDetails enrichi (tu peux utiliser ta factory ici aussi)
        var problemDetailsFactory =
            context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        var validationProblemDetails = problemDetailsFactory.CreateValidationProblemDetails(
            context.HttpContext,
            context.ModelState,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Model validation failed",
            detail: "See the errors property for details."
        );

        // Retourne un ObjectResult avec le contenu problem details et le bon code HTTP
        return new ObjectResult(validationProblemDetails)
        {
            StatusCode = validationProblemDetails.Status,
        };
    };
});

// Lier JwtSettings à l’IOptions
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Récupérer JwtSettings pour configurer JWT Bearer
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

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

builder.Services.AddApplication().AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

Log.Information("Starting application...");

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

Log.CloseAndFlush();

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PulseERP.Infrastructure.Database;

public class CoreDbContextFactory : IDesignTimeDbContextFactory<CoreDbContext>
{
    public CoreDbContext CreateDbContext(string[] args)
    {
        // Trouver le répertoire de l'API
        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Current directory: {basePath}");

        // Si pas répertoire Infrastructure, on remonte d'un niveau
        if (basePath.EndsWith("PulseERP.Infrastructure"))
            basePath = Path.Combine(basePath, "..", "PulseERP.API");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
                optional: true
            )
            .AddUserSecrets("19ad8bae-7b68-4cf5-b5c8-aa36453d2338")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<CoreDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CoreDbContext(optionsBuilder.Options);
    }
}

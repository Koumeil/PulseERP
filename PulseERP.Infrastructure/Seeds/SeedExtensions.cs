using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Security.Interfaces;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Adresses;
using PulseERP.Infrastructure.Database;
using PulseERP.Infrastructure.Identity;

namespace PulseERP.Infrastructure.Seeds;

public static class SeedExtensions
{
    public static async Task SeedIfEmptyAsync(
        this CoreDbContext context,
        ILogger logger,
        IPasswordService passwordService
    )
    {
        try
        {
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Seeds", "seeds.json");

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException($"Seed file not found: {jsonPath}");

            var json = await File.ReadAllTextAsync(jsonPath);
            var data = JsonSerializer.Deserialize<RootSeed>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            if (data is null)
            {
                logger.LogWarning("No seed data found in seeds.json.");
                return;
            }

            // Products
            // Récupère tous les brands existants (en base)
            var existingBrands = await context.Brands.ToListAsync();
            // Dictionnaire pour éviter de créer deux fois le même brand pendant le seed
            var brandsDict = existingBrands.ToDictionary(
                b => b.Name,
                StringComparer.OrdinalIgnoreCase
            );

            if (!await context.Products.AnyAsync())
            {
                foreach (var p in data.Products)
                {
                    // Vérifie si le brand existe déjà (DB ou déjà ajouté pendant ce seed)
                    if (!brandsDict.TryGetValue(p.Brand, out var brand))
                    {
                        brand = Brand.Create(p.Brand);
                        context.Brands.Add(brand);
                        brandsDict[p.Brand] = brand; // Ajoute au dict pour les prochains passages
                    }

                    context.Products.Add(
                        Product.Create(
                            p.Name,
                            p.Description,
                            brand,
                            p.Price,
                            p.Quantity,
                            p.IsService
                        )
                    );
                }
            }

            // Customers
            if (!await context.Customers.AnyAsync())
            {
                foreach (var c in data.Customers)
                {
                    var email = EmailAddress.Create(c.Email);
                    var phone = Phone.Create(c.Phone);
                    // Compose le bon format pour l’adresse
                    var rawAddress = $"{c.Street}, {c.City}, {c.ZipCode}, {c.Country}";
                    var address = Address.Create(rawAddress);
                    var customerType = Enum.Parse<CustomerType>(c.Type, ignoreCase: true);
                    var customerStatus = Enum.Parse<CustomerStatus>(c.Status, ignoreCase: true);

                    context.Customers.Add(
                        Customer.Create(
                            c.FirstName,
                            c.LastName,
                            c.CompanyName,
                            email,
                            phone,
                            address,
                            customerType,
                            customerStatus,
                            c.FirstContactDate,
                            c.IsVIP,
                            c.Industry,
                            c.Source
                        )
                    );
                }
            }

            // Users
            if (!await context.Users.AnyAsync())
            {
                foreach (var u in data.Users)
                {
                    var email = EmailAddress.Create(u.Email);
                    var phone = Phone.Create(u.Phone);
                    var hashedPassword = passwordService.HashPassword(u.Password);

                    context.Users.Add(
                        User.Create(u.FirstName, u.LastName, email, phone, hashedPassword)
                    );
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Seed data successfully inserted (if needed).");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SeedExtensions] Seeding failed: {Message}", ex.Message);
#if DEBUG
            System.Diagnostics.Debug.WriteLine("[SeedExtensions] Exception: " + ex);
#endif
        }
    }
}

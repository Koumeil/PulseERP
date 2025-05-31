using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Product;

namespace PulseERP.Infrastructure.Database;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options) { }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Brand> Brands { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ==============================================
        // User - Configuration
        // ==============================================
        builder.Entity<User>(u =>
        {
            // Propriétés standards
            u.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            u.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            u.Property(x => x.IsActive).HasDefaultValue(true);

            // Email (Value Object) avec conversion
            u.Property(x => x.Email)
                .IsRequired()
                .HasConversion(
                    v => v.Value, // Stocker en base : Email.Value (string)
                    v => Email.Create(v) // Relecture : créer un Email à partir du string
                )
                .HasMaxLength(255);

            // Phone (Value Object) avec conversion
            u.Property(x => x.Phone)
                .IsRequired()
                .HasConversion(
                    v => v.Value, // Stocker en base : Phone.Value (string)
                    v => Phone.Create(v) // Relecture : créer un Phone à partir du string
                )
                .HasMaxLength(20);

            // UserRole (Value Object) comme Owned Entity
            u.OwnsOne(
                x => x.Role,
                role =>
                {
                    role.Property(r => r.RoleName)
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnName("RoleName");
                }
            );

            // Index sur Email
            u.HasIndex(x => x.Email).IsUnique();
        });

        // ==============================================
        // Customer - Configuration
        // ==============================================
        builder.Entity<Customer>(c =>
        {
            // Propriétés standards
            c.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            c.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            c.Property(x => x.IsActive).HasDefaultValue(true);

            // Email (Value Object) avec conversion
            c.Property(x => x.Email)
                .IsRequired()
                .HasConversion(
                    v => v.Value, // Stocker en base : Email.Value
                    v => Email.Create(v) // Relecture : recréer Email
                )
                .HasMaxLength(255);

            // Phone (Value Object) avec conversion
            c.Property(x => x.Phone)
                .IsRequired()
                .HasConversion(
                    v => v.Value, // Stocker en base : Phone.Value
                    v => Phone.Create(v) // Relecture : recréer Phone
                )
                .HasMaxLength(20);

            // Address en tant qu’Owned Entity (Value Object)
            c.OwnsOne(
                x => x.Address,
                a =>
                {
                    a.Property(p => p.Street).HasColumnName("Street").HasMaxLength(100);
                    a.Property(p => p.City).HasColumnName("City").HasMaxLength(50);
                    a.Property(p => p.ZipCode).HasColumnName("PostalCode").HasMaxLength(20);
                    a.Property(p => p.Country).HasColumnName("Country").HasMaxLength(50);
                }
            );

            // Index sur Email
            c.HasIndex(x => x.Email).IsUnique();
        });

        // ==============================================
        // Product - Configuration
        // ==============================================
        builder.Entity<Product>(p =>
        {
            // Conversion pour ProductName ⇄ string
            p.Property(x => x.Name)
                .IsRequired()
                .HasConversion(
                    v => v.Value, // quand on écrit en base, on stocke ProductName.Value (string)
                    v => new ProductName(v) // quand on lit de la base, on reconstruit ProductName(v)
                )
                .HasMaxLength(100);

            // Conversion pour ProductDescription ⇄ string
            p.Property(x => x.Description)
                .HasConversion(
                    v => v == null ? null : v.Value,
                    v => v == null ? null : new ProductDescription(v)
                )
                .HasMaxLength(500)
                .IsUnicode(false);

            p.Property(x => x.IsActive).HasDefaultValue(true);
            p.Property(x => x.Quantity).HasDefaultValue(0);

            // Conversion pour Money ⇄ decimal
            p.Property(x => x.Price)
                .HasConversion(v => v.Value, v => new Money(v))
                .HasColumnType("decimal(18,2)");

            // Relation avec Brand
            p.HasOne(x => x.Brand)
                .WithMany()
                .HasForeignKey("BrandId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ==============================================
        // Brand - Configuration
        // ==============================================
        builder.Entity<Brand>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();

            // 2) Nom obligatoire, longueur max
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);

            // 3) Index sur Name pour garantir l'unicité des noms de marque
            b.HasIndex(x => x.Name).IsUnique();

            // 4) Par convention, IsActive par défaut true
            b.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // ==============================================
        // RefreshToken - Configuration
        // ==============================================
        builder.Entity<RefreshToken>(rt =>
        {
            rt.HasIndex(x => x.Token).IsUnique();
            rt.HasIndex(x => x.UserId);
            rt.Property(x => x.Expires).IsRequired();

            // Pour garder l’ID auto-généré, EF Core inferera la clé primaire automatiquement
            // à partir de la convention (Id comme PK).
        });
    }
}

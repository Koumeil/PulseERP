using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Adresses;
using PulseERP.Domain.ValueObjects.Products;

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

        // Appliquer le converter global pour DateTime ↔ UTC/Local
        UtcLocalDateTimeConverter.ApplyConversions(builder);

        // ==============================================
        // User - Configuration
        // ==============================================
        builder.Entity<User>(u =>
        {
            // Propriétés standards
            u.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            u.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            u.Property(x => x.IsActive).HasDefaultValue(true);

            // Email (Value Object) ↔ string - CORRECTION ICI
            u.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasConversion(
                    vo => vo.ToString(), // Convertit l'EmailAddress en string
                    str => EmailAddress.Create(str) // Convertit la string en EmailAddress
                );

            // Phone (Value Object) ↔ string
            u.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(vo => vo.Value, str => Phone.Create(str));

            // Role (Value Object) ↔ string (colonne "RoleName")
            u.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("RoleName")
                .HasConversion(vo => vo.Value, str => Role.Create(str));

            u.HasIndex(x => x.Email).IsUnique();
        });

        // ==============================================
        // Customer - Configuration
        // ==============================================
        builder.Entity<Customer>(c =>
        {
            c.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            c.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            c.Property(x => x.IsActive).HasDefaultValue(true);

            // Email (Value Object) ↔ string - CORRECTION ICI AUSSI
            c.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasConversion(vo => vo.ToString(), str => EmailAddress.Create(str));

            // Phone (Value Object) ↔ string
            c.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(vo => vo.Value, str => Phone.Create(str));

            // Address en tant qu'Owned Entity (Value Object)
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

            c.HasIndex(x => x.Email).IsUnique();
        });

        // ==============================================
        // Product - Configuration
        // ==============================================

        builder.Entity<Product>(p =>
        {
            // ProductName (Value Object) ↔ string
            p.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasConversion(vo => vo.Value, str => ProductName.Create(str));

            // Relation avec Brand - mapping explicite, résout tout conflit BrandId/BrandId1
            p.HasOne(x => x.Brand)
                .WithMany() // <= PAS d'argument ici !
                .HasForeignKey(x => x.BrandId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // ProductDescription (Value Object) ↔ string?
            p.Property(x => x.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasConversion(
                    vo => vo == null ? null : vo.Value,
                    str => str == null ? null : ProductDescription.Create(str)
                );

            p.Property(x => x.IsActive).HasDefaultValue(true);
            p.Property(x => x.Quantity).HasDefaultValue(0);

            // Money (Value Object) ↔ decimal
            p.Property(x => x.Price)
                .HasColumnType("decimal(18,2)")
                .HasConversion(vo => vo.Value, dec => new Money(dec));
        });

        // ==============================================
        // Brand - Configuration
        // ==============================================
        builder.Entity<Brand>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.HasMany(b => b.Products).WithOne("Brand").HasForeignKey("BrandId").IsRequired();
        });

        // ==============================================
        // RefreshToken - Configuration
        // ==============================================
        builder.Entity<RefreshToken>(rt =>
        {
            rt.HasIndex(x => x.Token).IsUnique();
            rt.HasIndex(x => x.UserId);
            rt.Property(x => x.Expires).IsRequired();
        });
    }
}

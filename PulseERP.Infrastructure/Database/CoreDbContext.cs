using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.VO;

namespace PulseERP.Infrastructure.Database;

public class CoreDbContext(DbContextOptions<CoreDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Conversion des Value Objects Money, Currency
        modelBuilder.Owned<Money>();
        modelBuilder.Owned<Currency>();

        // 2. Conversion globale DateTime ↔ UTC/Local
        UtcLocalDateTimeConverter.ApplyConversions(modelBuilder);

        // ==============================================
        // Configuration Entity : User
        // ==============================================
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasConversion(vo => vo.ToString(), str => new EmailAddress(str));

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(vo => vo.Value, str => new Phone(str));

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("RoleName")
                .HasConversion(vo => vo.Value, str => new Role(str));

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ==============================================
        // Configuration Entity : Customer
        // ==============================================
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasConversion(vo => vo.ToString(), str => new EmailAddress(str));

            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(vo => vo.Value, str => new Phone(str));

            entity.OwnsOne(
                e => e.Address,
                addressBuilder =>
                {
                    addressBuilder.Property(a => a.Street).HasColumnName("Street");
                    addressBuilder.Property(a => a.City).HasColumnName("City");
                    addressBuilder.Property(a => a.PostalCode).HasColumnName("PostalCode");
                    addressBuilder.Property(a => a.Country).HasColumnName("Country");
                });

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ==============================================
        // Configuration Entity : Product
        // ==============================================
        var currencyConverter = new ValueConverter<Currency, string>(
            v => v.Code,
            v => new Currency(v)
        );

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Name)
                .HasConversion(v => v.Value, v => new ProductName(v))
                .HasMaxLength(200);

            entity.HasOne(e => e.Brand)
                .WithMany()
                .HasForeignKey(e => e.BrandId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasConversion(
                    vo => vo == null ? null : vo.Value,
                    str => str == null ? null : new ProductDescription(str)
                );

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsService).HasDefaultValue(false);

            entity.OwnsOne(
                e => e.Price,
                priceBuilder =>
                {
                    priceBuilder
                        .Property(p => p.Amount)
                        .HasColumnName("PriceAmount")
                        .HasPrecision(18, 2)
                        .IsRequired();

                    priceBuilder
                        .Property(p => p.Currency)
                        .HasColumnName("PriceCurrency")
                        .IsRequired()
                        .HasConversion(currencyConverter);
                });
        });

        // ==============================================
        // Configuration Entity : Inventory
        // ==============================================
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasDefaultValue(0);

            entity.HasOne<Product>()
                .WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(e => e.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Movements)
                .WithOne(m => m.Inventory)
                .HasForeignKey(m => m.InventoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==============================================
        // Configuration Entity : InventoryMovement
        // ==============================================
        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuantityChange).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.OccurredAt).IsRequired();
            entity.Property(e => e.Reason).IsRequired();

            entity.HasOne(e => e.Inventory)
                .WithMany(i => i.Movements)
                .HasForeignKey(e => e.InventoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==============================================
        // Configuration Entity : Brand
        // ==============================================
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasMany(e => e.Products)
                .WithOne(p => p.Brand)
                .HasForeignKey(p => p.BrandId)
                .IsRequired();
        });

        // ==============================================
        // Configuration Entity : RefreshToken
        // ==============================================
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Expires).IsRequired();
        });
    }
}
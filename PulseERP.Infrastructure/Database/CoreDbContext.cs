using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.VO;

namespace PulseERP.Infrastructure.Database;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. Conversion des Value Objects Money, Currency
        builder.Owned<Money>();
        builder.Owned<Currency>();

        // 2. Conversion globale DateTime ↔ UTC/Local
        UtcLocalDateTimeConverter.ApplyConversions(builder);

        // ==============================================
        // Configuration Entity : User
        // ==============================================
        builder.Entity<User>(u =>
        {
            u.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            u.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            u.Property(x => x.IsActive).HasDefaultValue(true);

            u.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasConversion(vo => vo.ToString(), str => new EmailAddress(str));

            u.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(vo => vo.Value, str => new Phone(str));

            u.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("RoleName")
                .HasConversion(vo => vo.Value, str => new Role(str));

            u.HasIndex(x => x.Email).IsUnique();
        });

        // ==============================================
        // Configuration Entity : Customer
        // ==============================================
        builder.Entity<Customer>(c =>
        {
            c.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            c.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            c.Property(x => x.IsActive).HasDefaultValue(true);

            c.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasConversion(vo => vo.ToString(), str => new EmailAddress(str));

            c.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(vo => vo.Value, str => new Phone(str));

            c.OwnsOne(
                x => x.Address,
                a =>
                {
                    a.Property(ad => ad.Street).HasColumnName("Street");
                    a.Property(ad => ad.City).HasColumnName("City");
                    a.Property(ad => ad.PostalCode).HasColumnName("PostalCode");
                    a.Property(ad => ad.Country).HasColumnName("Country");
                }
            );

            c.HasIndex(x => x.Email).IsUnique();
        });

        // ==============================================
        // Configuration Entity : Product
        // ==============================================
        var currencyConverter = new ValueConverter<Currency, string>(
            v => v.Code, // Convertir Currency en string
            v => new Currency(v) // Convertir string en Currency
        );

        builder.Entity<Product>(p =>
        {
            p.Property(p => p.Name)
                .HasConversion(v => v.Value, v => new ProductName(v))
                .HasMaxLength(200);

            p.HasOne(x => x.Brand)
                .WithMany()
                .HasForeignKey(x => x.BrandId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            p.Property(p => p.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasConversion(
                    vo => vo == null ? null : vo.Value,
                    str => str == null ? null : new ProductDescription(str)
                );

            p.Property(p => p.IsActive).HasDefaultValue(true);
            p.Property(p => p.IsService).HasDefaultValue(false);

            // Configuration de Money avec OwnsOne
            p.OwnsOne(
                x => x.Price,
                price =>
                {
                    price
                        .Property(m => m.Amount)
                        .HasColumnName("PriceAmount")
                        .HasPrecision(18, 2)
                        .IsRequired();

                    price
                        .Property(m => m.Currency)
                        .HasColumnName("PriceCurrency")
                        .IsRequired()
                        .HasConversion(currencyConverter);
                }
            );
        });

        // ==============================================
        // Configuration Entity : Inventory
        builder.Entity<Inventory>(i =>
        {
            i.HasKey(x => x.Id);
            i.Property(x => x.Quantity).HasDefaultValue(0);

            // 1:1 vers Product (FK = Inventory.ProductId → Products.Id)
            i.HasOne<Product>()
                .WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(i => i.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // Configure cascade delete for Inventory

            // 1:N vers InventoryMovements, en utilisant Inventory.Id
            i.HasMany(i => i.Movements)
                .WithOne(m => m.Inventory)
                .HasForeignKey(m => m.InventoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // Configure cascade delete for InventoryMovements
        });

        // ==============================================
        // Configuration Entity : InventoryMovement
        builder.Entity<InventoryMovement>(m =>
        {
            m.HasKey(x => x.Id);
            m.Property(x => x.QuantityChange).IsRequired();
            m.Property(x => x.Type).IsRequired();
            m.Property(x => x.OccurredAt).IsRequired();
            m.Property(x => x.Reason).IsRequired();

            // Relation vers Inventory (FK = InventoryMovement.InventoryId → Inventories.Id)
            m.HasOne(x => x.Inventory)
                .WithMany(i => i.Movements)
                .HasForeignKey(x => x.InventoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // Ensure cascading delete here too
        });

        // ==============================================
        // Configuration Entity : Brand
        builder.Entity<Brand>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasMany(b => b.Products)
                .WithOne(p => p.Brand)
                .HasForeignKey(p => p.BrandId)
                .IsRequired();
        });

        // ==============================================
        // Configuration Entity : RefreshToken
        builder.Entity<RefreshToken>(rt =>
        {
            rt.HasIndex(x => x.Token).IsUnique();
            rt.HasIndex(x => x.UserId);
            rt.Property(x => x.Expires).IsRequired();
        });
    }
}

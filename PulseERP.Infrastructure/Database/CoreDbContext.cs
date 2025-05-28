using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;
using PulseERP.Infrastructure.Identity.Entities;

namespace PulseERP.Infrastructure.Database;

public class CoreDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options) { }

    // DbSets
    public DbSet<User> DomainUsers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Brand> Brands { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ==============================================
        // Identity - Configuration ApplicationUser
        // ==============================================
        builder.Entity<ApplicationUser>(entity =>
        {
            entity
                .HasOne(u => u.DomainUser)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.DomainUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Désactiver les champs Email standard d'Identity si nécessaire
            entity.Ignore(u => u.Email);
            entity.Ignore(u => u.EmailConfirmed);
        });

        // ==============================================
        // User - Configuration
        // ==============================================
        builder.Entity<User>(u =>
        {
            // Propriétés standards
            u.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            u.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            u.Property(x => x.IsActive).HasDefaultValue(true);

            // Value Objects avec conversion
            u.Property(x => x.Email)
                .IsRequired()
                .HasConversion(v => v.Value, v => Email.Create(v))
                .HasMaxLength(255);

            u.Property(x => x.Phone)
                .IsRequired()
                .HasConversion(v => v.Value, v => PhoneNumber.Create(v))
                .HasMaxLength(20);

            // Index
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

            // Value Objects avec conversion
            c.Property(x => x.Email)
                .IsRequired()
                .HasConversion(v => v.Value, v => Email.Create(v))
                .HasMaxLength(255);

            c.Property(x => x.Phone)
                .IsRequired()
                .HasConversion(v => v.Value, v => PhoneNumber.Create(v))
                .HasMaxLength(20);

            // Address comme Owned Entity
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

            // Index
            c.HasIndex(x => x.Email).IsUnique();
        });

        // ==============================================
        // Product - Configuration
        // ==============================================
        builder.Entity<Product>(p =>
        {
            p.Property(x => x.Name).IsRequired().HasMaxLength(100);
            p.Property(x => x.Description).HasMaxLength(500);
            p.Property(x => x.IsActive).HasDefaultValue(true);
            p.Property(x => x.Quantity).HasDefaultValue(0);

            // Money Value Object
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
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.Name).IsUnique();
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
} // using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore;
// using PulseERP.Domain.Entities;
// using PulseERP.Domain.ValueObjects;
// using PulseERP.Infrastructure.Identity.Entities;

// namespace PulseERP.Infrastructure.Database;

// public class CoreDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
// {
//     public CoreDbContext(DbContextOptions<CoreDbContext> options)
//         : base(options) { }

//     public DbSet<User> DomainUsers { get; set; }
//     public DbSet<RefreshToken> RefreshTokens { get; set; }
//     public DbSet<Product> Products { get; set; }
//     public DbSet<Customer> Customers { get; set; }
//     public DbSet<Brand> Brands { get; set; }

//     protected override void OnModelCreating(ModelBuilder builder)
//     {
//         base.OnModelCreating(builder);

//         // Relation
//         builder
//             .Entity<ApplicationUser>()
//             .HasOne(u => u.DomainUser)
//             .WithOne()
//             .HasForeignKey<ApplicationUser>(u => u.DomainUserId);

//         // ValueObjects (version corrigée)
//         builder.Entity<User>(u =>
//         {
//             u.Property(x => x.Email).IsRequired().HasConversion(v => v.Value, v => Email.Create(v));

//             u.Property(x => x.Phone)
//                 .IsRequired()
//                 .HasConversion(v => v.Value, v => PhoneNumber.Create(v));
//         });

//         builder.Entity<Product>(entity =>
//         {
//             entity
//                 .Property(p => p.Price)
//                 .HasConversion(
//                     v => v.Value, // Convert Money → decimal (to store)
//                     v => new Money(v) // Convert decimal → Money (when reading)
//                 );
//         });

//         builder.Entity<Customer>(c =>
//         {
//             // Adresse en tant que Value Object possédé
//             c.OwnsOne(
//                 c => c.Address,
//                 a =>
//                 {
//                     a.Property(p => p.Street).HasColumnName("Street");
//                     a.Property(p => p.City).HasColumnName("City");
//                     a.Property(p => p.ZipCode).HasColumnName("PostalCode");
//                     a.Property(p => p.Country).HasColumnName("Country");
//                 }
//             );

//             // Email (ValueObject)
//             c.Property(x => x.Email)
//                 .IsRequired()
//                 .HasConversion(v => v.Value, v => Email.Create(v));

//             // Phone (non-nullable ValueObject)
//             c.Property(x => x.Phone)
//                 .IsRequired()
//                 .HasConversion(v => v.Value, v => PhoneNumber.Create(v));
//         });

//         builder.Entity<Product>(entity =>
//         {
//             entity.HasOne(p => p.Brand).WithMany().HasForeignKey("BrandId").IsRequired();
//         });
//     }
// }

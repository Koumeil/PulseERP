using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;
using PulseERP.Infrastructure.Identity;

namespace PulseERP.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> DomainUsers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Brand> Brands { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Relation
        builder
            .Entity<ApplicationUser>()
            .HasOne(u => u.DomainUser)
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.DomainUserId);

        // ValueObjects (version minimaliste)
        builder.Entity<User>(u =>
        {
            u.Property(x => x.Email).HasConversion(v => v.Value, v => new Email(v));
            u.Property(x => x.Phone)
                .HasConversion(v => v!.Value, v => v != null ? new PhoneNumber(v) : null);
        });

        builder.Entity<Product>(entity =>
        {
            entity
                .Property(p => p.Price)
                .HasConversion(
                    v => v.Value, // Convert Money → decimal (to store)
                    v => new Money(v) // Convert decimal → Money (when reading)
                );
        });

        builder.Entity<Customer>(c =>
        {
            // Adresse en tant que Value Object possédé
            c.OwnsOne(
                c => c.Address,
                a =>
                {
                    a.Property(p => p.Street).HasColumnName("Street");
                    a.Property(p => p.City).HasColumnName("City");
                    a.Property(p => p.ZipCode).HasColumnName("PostalCode");
                    a.Property(p => p.Country).HasColumnName("Country");
                }
            );

            c.Property(x => x.Email).HasConversion(v => v.Value, v => new Email(v));

            c.Property(x => x.Phone)
                .HasConversion(v => v!.Value, v => v != null ? new PhoneNumber(v) : null);
        });

        builder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.Brand).WithMany().HasForeignKey("BrandId").IsRequired();
        });
    }
}

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
    }
}

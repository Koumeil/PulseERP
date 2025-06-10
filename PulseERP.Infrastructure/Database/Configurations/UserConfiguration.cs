using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.VO;

namespace PulseERP.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
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
    }
}
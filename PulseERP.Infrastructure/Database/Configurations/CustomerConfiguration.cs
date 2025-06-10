using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseERP.Domain.Entities;
using PulseERP.Domain.VO;

namespace PulseERP.Infrastructure.Database.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> entity)
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

        entity.OwnsOne(e => e.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasColumnName("Street");
            addressBuilder.Property(a => a.City).HasColumnName("City");
            addressBuilder.Property(a => a.PostalCode).HasColumnName("PostalCode");
            addressBuilder.Property(a => a.Country).HasColumnName("Country");
        });

        entity.HasIndex(e => e.Email).IsUnique();
    }
}
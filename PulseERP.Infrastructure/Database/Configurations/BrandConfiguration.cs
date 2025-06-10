namespace PulseERP.Infrastructure.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> entity)
    {
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.HasIndex(e => e.Name).IsUnique();
        entity.Property(e => e.IsActive).HasDefaultValue(true);

        entity.HasMany(e => e.Products)
            .WithOne(p => p.Brand)
            .HasForeignKey(p => p.BrandId)
            .IsRequired();
    }
}

namespace PulseERP.Infrastructure.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Quantity).HasDefaultValue(0);

        entity.HasOne<Product>()
            .WithOne(p => p.Inventory)
            .HasForeignKey<Inventory>(e => e.ProductId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PulseERP.Infrastructure.Database.Configurations;

using Domain.VO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Domain.Entities;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        var currencyConverter = new ValueConverter<Currency, string>(
            v => v.Code,
            v => new Currency(v)
        );

        var productNameComparer = new ValueComparer<ProductName>(
            (p1, p2) => p1.Value == p2.Value,
            p => p.Value.GetHashCode(),
            p => new ProductName(p.Value)
        );

        entity.Property(e => e.Name)
            .HasConversion(v => v.Value, v => new ProductName(v))
            .HasMaxLength(200)
            .Metadata.SetValueComparer(productNameComparer);


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

        entity.OwnsOne(e => e.Price, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount)
                .HasColumnName("PriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(p => p.Currency)
                .HasColumnName("PriceCurrency")
                .IsRequired()
                .HasConversion(currencyConverter);
        });
    }
}
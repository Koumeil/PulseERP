namespace PulseERP.Infrastructure.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<TokenEntity>
{
    public void Configure(EntityTypeBuilder<TokenEntity> entity)
    {
        entity.HasIndex(e => e.Token).IsUnique();
        entity.HasIndex(e => e.UserId);
        entity.Property(e => e.Expires).IsRequired();
        entity.Property(e => e.TokenType).HasConversion<string>();
    }
}

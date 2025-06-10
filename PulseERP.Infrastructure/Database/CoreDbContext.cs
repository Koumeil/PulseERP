using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Entities;
using PulseERP.Domain.VO;
using PulseERP.Infrastructure.Database.Configurations;

namespace PulseERP.Infrastructure.Database;

public class CoreDbContext(DbContextOptions<CoreDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<TokenEntity> RefreshTokens { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Inventory> Inventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Owned<Money>();
        modelBuilder.Owned<Currency>();

        // 2. Conversion global DateTime ↔ UTC/Local
        UtcLocalDateTimeConverter.ApplyConversions(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryConfiguration());
        modelBuilder.ApplyConfiguration(new BrandConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }


    public async Task<int> SaveChangesAndDispatchEventsAsync(IMediator mediator, CancellationToken cancellationToken = default)
    {
        // Get all entities with domain events
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        // Extract all domain events
        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        // Clear domain events to avoid duplicate publishing
        domainEntities.ForEach(e => e.ClearDomainEvents());

        // Persist changes to DB
        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events through MediatR
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);

            await mediator.Publish((INotification)notification, cancellationToken);
        }


        return result;
    }

}
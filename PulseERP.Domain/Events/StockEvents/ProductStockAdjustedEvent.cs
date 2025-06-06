using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.StockEvents;

public sealed class ProductStockAdjustedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public int NewQuantity { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductStockAdjustedEvent(Guid productId, int newQuantity)
    {
        ProductId = productId;
        NewQuantity = newQuantity;
    }
}

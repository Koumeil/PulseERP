namespace PulseERP.Domain.Entities;

using PulseERP.Domain.Enums.Inventory;
using Errors;
using Events.ProductEvents;
using Events.StockEvents;
using Extensions;

public sealed class Inventory : BaseEntity
{
    public Guid ProductId { get; }

    public int Quantity { get; private set; }


    private Inventory() { }

    public Inventory(Guid productId, int initialQuantity)
    {
        if (initialQuantity < 0)
            throw new DomainValidationException("Initial quantity must be non-negative.");

        ProductId = productId;
        Quantity = initialQuantity;

    }

    public void Restock(int amount)
    {
        if (amount <= 0)
            throw new DomainValidationException("Restock amount must be positive.");

        Quantity += amount;
        AddDomainEvent(new ProductRestockedEvent(ProductId, amount));
    }

    public void Decrease(int amount)
    {
        if (amount <= 0)
            throw new DomainValidationException("Amount to decrease must be positive.");
        if (amount > Quantity)
            throw new DomainValidationException("Cannot decrease more than current quantity.");

        Quantity -= amount;
        AddDomainEvent(new ProductStockDecreasedEvent(ProductId, amount));
    }

    public void AdjustTo(int newQuantity, InventoryMovementType? overrideType = null)
    {
        if (newQuantity < 0)
            throw new DomainValidationException("Adjusted quantity must be non-negative.");

        var delta = newQuantity - Quantity;
        if (delta == 0)
            return;

        Quantity = newQuantity;

        var type =
            overrideType
            ?? (
                delta > 0
                    ? InventoryMovementType.CorrectionIncrease
                    : InventoryMovementType.CorrectionDecrease
            );

        AddDomainEvent(new ProductStockAdjustedEvent(ProductId, newQuantity));
    }

    public bool IsLowStock(int threshold = 5) => Quantity <= threshold;

    public bool IsOutOfStock() => Quantity == 0;

}

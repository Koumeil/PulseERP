namespace PulseERP.Domain.Entities;

using PulseERP.Domain.Common;
using PulseERP.Domain.Enums.Inventory;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Events.ProductEvents;
using PulseERP.Domain.Events.StockEvents;
using PulseERP.Domain.Extensions;

public sealed class Inventory : BaseEntity
{
    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    private readonly List<InventoryMovement> _movements = new();
    public IReadOnlyCollection<InventoryMovement> Movements => _movements.AsReadOnly();

    private Inventory() { }

    public Inventory(Guid productId, int initialQuantity)
    {
        if (initialQuantity < 0)
            throw new DomainValidationException("Initial quantity must be non-negative.");

        ProductId = productId;
        Quantity = initialQuantity;

        AddMovement(initialQuantity, InventoryMovementType.InitialStock);
    }

    public void Restock(int amount)
    {
        if (amount <= 0)
            throw new DomainValidationException("Restock amount must be positive.");

        Quantity += amount;
        AddMovement(amount, InventoryMovementType.Inbound);
        AddDomainEvent(new ProductRestockedEvent(ProductId, amount));
    }

    public void Decrease(int amount)
    {
        if (amount <= 0)
            throw new DomainValidationException("Amount to decrease must be positive.");
        if (amount > Quantity)
            throw new DomainValidationException("Cannot decrease more than current quantity.");

        Quantity -= amount;
        AddMovement(-amount, InventoryMovementType.Outbound);
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

        AddMovement(delta, type);
        AddDomainEvent(new ProductStockAdjustedEvent(ProductId, newQuantity));
    }

    public bool IsLowStock(int threshold = 5) => Quantity <= threshold;

    public bool IsOutOfStock() => Quantity == 0;

    public void HandleReturn(int quantity)
    {
        if (quantity <= 0)
            throw new DomainValidationException("Return amount must be positive.");

        Quantity += quantity;
        AddMovement(quantity, InventoryMovementType.Return);
        Quantity += quantity;
        AddMovement(quantity, InventoryMovementType.Return);
        AddDomainEvent(new ProductStockReturnedEvent(ProductId, quantity));
    }

    private void AddMovement(int quantityChange, InventoryMovementType type)
    {
        var movement = new InventoryMovement(
            quantityChange,
            type,
            DateTime.UtcNow,
            type.GetDescription()
        );
        _movements.Add(movement);
    }
}

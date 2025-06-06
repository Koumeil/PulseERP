namespace PulseERP.Domain.Extensions;

using PulseERP.Domain.Enums.Inventory;

public static class InventoryMovementTypeExtensions
{
    public static string GetDescription(this InventoryMovementType type) =>
        type switch
        {
            InventoryMovementType.InitialStock => "Initial stock",
            InventoryMovementType.Inbound => "Inbound movement",
            InventoryMovementType.Outbound => "Outbound movement",
            InventoryMovementType.CorrectionIncrease => "Stock correction (increase)",
            InventoryMovementType.CorrectionDecrease => "Stock correction (decrease)",
            InventoryMovementType.Return => "Product return",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown movement type"),
        };
}

using System.Text.Json.Serialization;
using PulseERP.Domain.Common;
using PulseERP.Domain.Enums.Inventory;

namespace PulseERP.Domain.Entities;

/// <summary>
/// Mouvements individuels de l'inventaire (entrée, sortie, correction).
/// </summary>
public sealed class InventoryMovement : BaseEntity
{
    public Guid InventoryId { get; private set; }
    public Inventory Inventory { get; private set; } = null!;
    public int QuantityChange { get; private set; }
    public InventoryMovementType Type { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private InventoryMovement() { }

    public InventoryMovement(
        int quantityChange,
        InventoryMovementType type,
        DateTime occurredAt,
        string reason
    )
    {
        QuantityChange = quantityChange;
        Type = type;
        OccurredAt = occurredAt;
        Reason = reason;
    }
}

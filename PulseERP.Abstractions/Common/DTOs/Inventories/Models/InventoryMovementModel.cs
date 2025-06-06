namespace PulseERP.Abstractions.Common.DTOs.Inventories.Models;

public sealed record InventoryMovementModel(
    Guid Id,
    Guid ProductId,
    int QuantityChange,
    string Type,
    DateTime OccurredAt,
    string Reason
);

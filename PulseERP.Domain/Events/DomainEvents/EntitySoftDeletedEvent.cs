using PulseERP.Domain.Common;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.DomainEvents;

/// <summary>
/// Exemple d’événement standard déclenché lorsqu’une entité est soft-deleted.
/// Utilisé pour éventuellement archiver, notifier, etc., dans l’infrastructure.
/// </summary>
public sealed class EntitySoftDeletedEvent : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Référence à l’entité qui a été supprimée en soft-delete.
    /// </summary>
    public BaseEntity Entity { get; }

    /// <summary>
    /// Crée un nouvel événement de soft-delete pour l’entité spécifiée.
    /// </summary>
    /// <param name="entity">Entité soft-deleted.</param>
    public EntitySoftDeletedEvent(BaseEntity entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }
}

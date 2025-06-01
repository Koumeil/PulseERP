namespace PulseERP.Domain.Entities;

/// <summary>
/// Base class for all aggregate roots and entities.
/// </summary>
public abstract class BaseEntity
{
    #region Properties

    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    #endregion

    #region Methods

    /// <summary>
    /// Marks the entity as updated by setting the UpdatedAt timestamp to current UTC.
    /// </summary>
    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;

    #endregion
}

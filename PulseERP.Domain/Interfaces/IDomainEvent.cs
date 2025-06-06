namespace PulseERP.Domain.Interfaces;

using System;

/// <summary>
/// Interface représentant un événement de domaine.
/// Implémenté par tous les événements spécifiques au domaine.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Timestamp UTC auquel l’événement a été créé.
    /// </summary>
    DateTime OccurredOn { get; }
}

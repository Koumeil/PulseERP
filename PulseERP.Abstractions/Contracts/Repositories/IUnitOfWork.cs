namespace PulseERP.Abstractions.Contracts.Repositories;

/// <summary>
/// Définit l’unité de travail pour regrouper les opérations atomiques.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persiste les changements dans la base de données dans une transaction.
    /// </summary>
    /// <param name="cancellationToken">Jeton d’annulation.</param>
    /// <returns>Nombre d’entrées affectées.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

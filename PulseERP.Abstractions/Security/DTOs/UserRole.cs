namespace PulseERP.Abstractions.Security.DTOs;

/// <summary>
/// Représente un rôle utilisateur exposé à l’UI/API.
/// <br/>— Simple texte immuable, sans logique métier.
/// </summary>
public sealed record UserRole(string Name);

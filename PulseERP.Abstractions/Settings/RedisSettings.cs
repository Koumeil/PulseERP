using System.ComponentModel.DataAnnotations;

namespace PulseERP.Abstractions.Settings;

/// <summary>
/// Paramètres pour la configuration de Redis (StackExchangeRedisCache).
/// </summary>
public sealed class RedisSettings
{
    /// <summary>
    /// Chaîne de connexion au serveur Redis (ex. "localhost:6379").
    /// </summary>
    [Required]
    public string Configuration { get; set; } = default!;

    /// <summary>
    /// Préfixe optionnel pour les clés Redis (ex. "PulseERP:").
    /// </summary>
    [Required]
    public string InstanceName { get; set; } = default!;
}

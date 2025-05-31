using System.ComponentModel.DataAnnotations;

namespace PulseERP.Abstractions.Settings;

/// <summary>Paramètres SMTP injectés via <c>IOptions&lt;SmtpSettings&gt;</c>.</summary>
public sealed class EmailSettings
{
    [Required]
    public string SenderName { get; init; } = null!;

    [Required]
    public string FromEmail { get; init; } = null!;

    [Required]
    public string MailServer { get; init; } = null!;

    [Range(1, 65535)]
    public int MailPort { get; init; }

    [Required]
    public string Password { get; init; } = null!;
}

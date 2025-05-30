using System.ComponentModel.DataAnnotations;

namespace PulseERP.Shared.Settings;

public class EmailSettings
{
    [Required]
    public string SenderName { get; set; } = null!;

    [Required]
    public string FromEmail { get; set; } = null!;

    [Required]
    public string MailServer { get; set; } = null!;

    [Range(1, 65535)]
    public int MailPort { get; set; }

    [Required]
    public string Password { get; set; } = null!;
}

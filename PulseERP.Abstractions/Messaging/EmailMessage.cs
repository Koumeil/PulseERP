namespace PulseERP.Abstractions.Messaging;

/// <summary>Message SMTP minimal (HTML & texte brut).</summary>
public sealed class EmailMessage
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }
    public string? TextBody { get; init; }
    public string? FromName { get; init; }
    public string? FromAddress { get; init; }
}

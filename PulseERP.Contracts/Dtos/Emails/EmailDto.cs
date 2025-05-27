namespace PulseERP.Contracts.Dtos.Emails;

public record EmailDto
{
    public string Value { get; init; }

    public EmailDto(string value)
    {
        Value = value;
    }
}

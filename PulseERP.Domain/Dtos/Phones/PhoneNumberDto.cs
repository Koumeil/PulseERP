namespace PulseERP.Domain.Dtos.Phones;

public record PhoneNumberDto
{
    public string Value { get; init; }

    public PhoneNumberDto(string value)
    {
        Value = value;
    }
}

namespace PulseERP.Domain.Errors;

public class BadRequestException : Exception
{
    public BadRequestException(string message)
        : base(message) { }
}

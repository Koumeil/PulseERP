namespace PulseERP.Domain.Errors;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Forbidden operation.")
        : base(message) { }
}

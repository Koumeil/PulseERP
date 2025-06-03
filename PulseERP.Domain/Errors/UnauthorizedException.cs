namespace PulseERP.Domain.Errors;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base(message) { }
}

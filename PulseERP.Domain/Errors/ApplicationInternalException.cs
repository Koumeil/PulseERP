namespace PulseERP.Domain.Errors;

/// <summary>
/// Exception levée lorsqu’une erreur serveur inattendue survient dans un service applicatif.
/// </summary>
public class ApplicationInternalException : Exception
{
    public ApplicationInternalException(string message, Exception innerException = null!)
        : base(message, innerException) { }
}

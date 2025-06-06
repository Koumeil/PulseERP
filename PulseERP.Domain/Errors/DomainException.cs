namespace PulseERP.Domain.Errors;

using System;

/// <summary>
/// Base class for all domain‐specific exceptions.
/// Use this as the parent type for exceptions that represent business‐rule or validation failures.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    protected DomainException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    protected DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}

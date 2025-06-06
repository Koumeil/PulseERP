using System;

namespace PulseERP.Domain.Errors;

/// <summary>
/// Exception thrown when a domain‐level invariant or validation rule is violated.
/// Use this to indicate that the input or state does not meet a business‐rule requirement.
/// </summary>
public sealed class DomainValidationException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the validation error.</param>
    public DomainValidationException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationException"/> class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the validation error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public DomainValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}

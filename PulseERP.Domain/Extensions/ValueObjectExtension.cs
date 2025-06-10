using PulseERP.Domain.Errors;

namespace PulseERP.Domain.Extensions;

public static class ValueObjectExtensions
{
    public static T TryCreateValueObject<T>(Func<T> factory, string fieldName, Dictionary<string, string[]> errors)
    {
        try
        {
            return factory();
        }
        catch (DomainValidationException ex)
        {
            errors.Add(fieldName, [ex.Message]);
            throw new ValidationException(errors);
        }
    }
}
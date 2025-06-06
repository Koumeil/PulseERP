namespace PulseERP.Domain.VO;

/// Base class for value objects to handle equality by comparing the sequence of components.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Returns the components that form the equality comparison.
    /// Les entités concrètes doivent implémenter cette méthode pour énumérer leurs props.
    /// </summary>
    /// <returns>IEnumerable of components.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        // Combine hash codes of all components
        unchecked
        {
            return GetEqualityComponents()
                .Select(component => component?.GetHashCode() ?? 0)
                .Aggregate(0, (current, next) => (current * 397) ^ next);
        }
    }

    /// <summary>
    /// Override == operator.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Override != operator.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}

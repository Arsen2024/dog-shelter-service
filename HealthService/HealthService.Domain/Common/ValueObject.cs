namespace HealthService.Domain.Common;

/// <summary>Базовий Value Object з порівнянням за значеннями (за зразком eShop, Ordering.Domain/SeedWork).</summary>
public abstract class ValueObject
{
    protected static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        if (left is null ^ right is null)
            return false;

        return left is null || left.Equals(right);
    }

    protected static bool NotEqualOperator(ValueObject? left, ValueObject? right) => !EqualOperator(left, right);

    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate(17, (hash, value) => unchecked(hash * 31 + value));

    public static bool operator ==(ValueObject? left, ValueObject? right) => EqualOperator(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => NotEqualOperator(left, right);
}

namespace EasyBuy.Domain.Primitives;

public static class Guard
{
    public static string AgainstNullOrEmpty(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);
        return value;
    }
    
    public static void AgainstNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
    }

    public static void AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
    }
    
    public static void AgainstOutOfRange<T>(T value, T min, T max, string parameterName) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"The value must be between {min} and {max}.");
        }
    }
    
    public static void AgainstOutOfRange(decimal value, decimal min, decimal max, string parameterName)
    {
        if (min > max)
            throw new ArgumentException("Min value cannot be greater than max value.");

        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {min} and {max}.");
    }

    public static void AgainstNullOrWhiteSpace(string street, string streetName)
    {
        throw new NotImplementedException();
    }
}
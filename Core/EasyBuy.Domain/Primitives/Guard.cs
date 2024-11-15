namespace EasyBuy.Domain.Primitives;

public static class Guard
{
    public static dynamic Against { get; set; }
    
    public static T AgainstNullOrEmpty<T>(T value, string parameterName) where T : class
    {
        if (string.IsNullOrWhiteSpace(value as string))
            throw new ArgumentException($"{parameterName} cannot be null or empty.");
        return value;
    }

    public static int AgainstNegativeOrZero(int value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be greater than zero.");
        return value;
    }
}
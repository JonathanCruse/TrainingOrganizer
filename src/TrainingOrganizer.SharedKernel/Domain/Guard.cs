using System.Text.RegularExpressions;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;

namespace TrainingOrganizer.SharedKernel.Domain;

public static partial class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{paramName} cannot be null or empty.");
        return value;
    }

    public static string AgainstOverflow(string value, int maxLength, string paramName)
    {
        if (value.Length > maxLength)
            throw new DomainException($"{paramName} cannot exceed {maxLength} characters.");
        return value;
    }

    public static string AgainstInvalidEmail(string value, string paramName)
    {
        AgainstNullOrWhiteSpace(value, paramName);
        if (!EmailRegex().IsMatch(value))
            throw new DomainException($"{paramName} is not a valid email address.");
        return value;
    }

    public static int AgainstNegative(int value, string paramName)
    {
        if (value < 0)
            throw new DomainException($"{paramName} cannot be negative.");
        return value;
    }

    public static int AgainstNonPositive(int value, string paramName)
    {
        if (value <= 0)
            throw new DomainException($"{paramName} must be positive.");
        return value;
    }

    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new DomainException($"{paramName} cannot be null.");
        return value;
    }

    public static void AgainstCondition(bool condition, string message)
    {
        if (condition)
            throw new DomainException(message);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}

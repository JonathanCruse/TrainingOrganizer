using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Membership.Domain.ValueObjects;

public sealed record Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        Value = Guard.AgainstInvalidEmail(value, nameof(value)).ToLowerInvariant();
    }

    public override string ToString() => Value;
}

using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Membership.ValueObjects;

public sealed record PhoneNumber : ValueObject
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        Value = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
    }

    public override string ToString() => Value;
}

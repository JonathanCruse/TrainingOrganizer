using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Membership.Domain.ValueObjects;

public sealed record PhoneNumber : ValueObject
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        Value = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
    }

    public override string ToString() => Value;
}

using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Facility.ValueObjects;

public sealed record LocationName : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

    public LocationName(string value)
    {
        Value = Guard.AgainstOverflow(
            Guard.AgainstNullOrWhiteSpace(value, nameof(value)),
            MaxLength, nameof(value));
    }

    public override string ToString() => Value;
}

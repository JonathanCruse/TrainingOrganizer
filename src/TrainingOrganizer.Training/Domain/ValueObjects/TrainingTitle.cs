using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Training.Domain.ValueObjects;

public sealed record TrainingTitle : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

    public TrainingTitle(string value)
    {
        Value = Guard.AgainstOverflow(
            Guard.AgainstNullOrWhiteSpace(value, nameof(value)),
            MaxLength, nameof(value));
    }

    public override string ToString() => Value;
}

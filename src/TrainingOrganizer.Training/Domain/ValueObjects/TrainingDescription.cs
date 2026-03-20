using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Training.Domain.ValueObjects;

public sealed record TrainingDescription : ValueObject
{
    public const int MaxLength = 4000;

    public string Value { get; }

    public TrainingDescription(string value)
    {
        Value = Guard.AgainstOverflow(value ?? string.Empty, MaxLength, nameof(value));
    }

    public override string ToString() => Value;
}

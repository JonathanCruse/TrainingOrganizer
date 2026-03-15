using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Training.ValueObjects;

public sealed record Capacity : ValueObject
{
    public int Min { get; }
    public int Max { get; }

    public Capacity(int min, int max)
    {
        Guard.AgainstNegative(min, nameof(min));
        Guard.AgainstNonPositive(max, nameof(max));
        Guard.AgainstCondition(max < min, "Capacity max must be greater than or equal to min.");

        Min = min;
        Max = max;
    }

    public bool IsFull(int currentCount) => currentCount >= Max;
    public bool HasMinimum(int currentCount) => currentCount >= Min;
}

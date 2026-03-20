namespace TrainingOrganizer.SharedKernel.Domain.ValueObjects;

public sealed record TimeSlot : ValueObject
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public TimeSlot(DateTimeOffset start, DateTimeOffset end)
    {
        Guard.AgainstCondition(end <= start, "TimeSlot end must be after start.");
        Start = start;
        End = end;
    }

    public bool OverlapsWith(TimeSlot other) =>
        Start < other.End && End > other.Start;

    public TimeSpan Duration => End - Start;
}

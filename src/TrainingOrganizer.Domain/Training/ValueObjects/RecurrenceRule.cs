using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Training.Enums;

namespace TrainingOrganizer.Domain.Training.ValueObjects;

public sealed record RecurrenceRule : ValueObject
{
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(8);

    public RecurrencePattern Pattern { get; }
    public DayOfWeek DayOfWeek { get; }
    public TimeOnly TimeOfDay { get; }
    public TimeSpan Duration { get; }
    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; }

    public RecurrenceRule(
        RecurrencePattern pattern,
        DayOfWeek dayOfWeek,
        TimeOnly timeOfDay,
        TimeSpan duration,
        DateOnly startDate,
        DateOnly? endDate = null)
    {
        Guard.AgainstCondition(duration <= TimeSpan.Zero, "Duration must be positive.");
        Guard.AgainstCondition(duration > MaxDuration, $"Duration cannot exceed {MaxDuration.TotalHours} hours.");
        Guard.AgainstCondition(endDate.HasValue && endDate.Value <= startDate,
            "EndDate must be after StartDate.");

        Pattern = pattern;
        DayOfWeek = dayOfWeek;
        TimeOfDay = timeOfDay;
        Duration = duration;
        StartDate = startDate;
        EndDate = endDate;
    }

    public IReadOnlyList<DateOnly> GetOccurrences(DateOnly from, DateOnly until)
    {
        var occurrences = new List<DateOnly>();
        var effectiveEnd = EndDate.HasValue && EndDate.Value < until ? EndDate.Value : until;
        var current = StartDate > from ? StartDate : from;

        // Align to the correct day of week
        while (current.DayOfWeek != DayOfWeek && current <= effectiveEnd)
            current = current.AddDays(1);

        var increment = Pattern switch
        {
            RecurrencePattern.Weekly => 7,
            RecurrencePattern.Biweekly => 14,
            RecurrencePattern.Monthly => 0, // handled separately
            _ => 7
        };

        while (current <= effectiveEnd)
        {
            occurrences.Add(current);

            if (Pattern == RecurrencePattern.Monthly)
            {
                // Next month, same day of week (find the first matching day)
                var nextMonth = current.AddMonths(1);
                nextMonth = new DateOnly(nextMonth.Year, nextMonth.Month, 1);
                while (nextMonth.DayOfWeek != DayOfWeek)
                    nextMonth = nextMonth.AddDays(1);
                current = nextMonth;
            }
            else
            {
                current = current.AddDays(increment);
            }
        }

        return occurrences;
    }
}

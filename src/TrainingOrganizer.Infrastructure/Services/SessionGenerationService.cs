using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Services;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Infrastructure.Services;

public sealed class SessionGenerationService : ISessionGenerationService
{
    private readonly ITrainingSessionRepository _sessionRepository;

    public SessionGenerationService(ITrainingSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<IReadOnlyList<TrainingSession>> GenerateSessionsAsync(
        RecurringTraining recurringTraining, DateOnly until, CancellationToken cancellationToken = default)
    {
        // Get existing sessions to avoid creating duplicates
        var existingSessions = await _sessionRepository
            .GetByRecurringTrainingIdAsync(recurringTraining.Id, cancellationToken);

        var existingDates = existingSessions
            .Select(s => DateOnly.FromDateTime(s.TimeSlot.Start.UtcDateTime))
            .ToHashSet();

        // Determine the range for generation
        var rule = recurringTraining.RecurrenceRule;
        var from = recurringTraining.LastGeneratedUntil?.AddDays(1) ?? rule.StartDate;
        var occurrences = rule.GetOccurrences(from, until);

        var newSessions = new List<TrainingSession>();

        foreach (var date in occurrences)
        {
            // Skip dates that already have sessions
            if (existingDates.Contains(date))
                continue;

            var startDateTime = date.ToDateTime(rule.TimeOfDay, DateTimeKind.Utc);
            var start = new DateTimeOffset(startDateTime, TimeSpan.Zero);
            var end = start.Add(rule.Duration);
            var timeSlot = new TimeSlot(start, end);

            var session = TrainingSession.CreateFromTemplate(
                recurringTraining.Id,
                timeSlot,
                recurringTraining.Template);

            newSessions.Add(session);
        }

        if (newSessions.Count > 0)
        {
            await _sessionRepository.AddRangeAsync(newSessions, cancellationToken);
        }

        return newSessions;
    }
}

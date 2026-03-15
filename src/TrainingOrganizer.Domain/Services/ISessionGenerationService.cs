using TrainingOrganizer.Domain.Training;

namespace TrainingOrganizer.Domain.Services;

public interface ISessionGenerationService
{
    Task<IReadOnlyList<TrainingSession>> GenerateSessionsAsync(
        RecurringTraining recurringTraining,
        DateOnly until,
        CancellationToken cancellationToken = default);
}

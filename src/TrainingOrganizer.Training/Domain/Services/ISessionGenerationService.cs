using TrainingOrganizer.Training.Domain;

namespace TrainingOrganizer.Training.Domain.Services;

public interface ISessionGenerationService
{
    Task<IReadOnlyList<TrainingSession>> GenerateSessionsAsync(
        RecurringTraining recurringTraining,
        DateOnly until,
        CancellationToken cancellationToken = default);
}

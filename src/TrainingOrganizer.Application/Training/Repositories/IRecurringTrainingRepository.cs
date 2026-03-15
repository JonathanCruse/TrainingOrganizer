using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Repositories;

public interface IRecurringTrainingRepository
{
    Task<RecurringTraining?> GetByIdAsync(RecurringTrainingId id, CancellationToken ct = default);
    Task<PagedList<RecurringTraining>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<RecurringTraining>> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(RecurringTraining recurringTraining, CancellationToken ct = default);
    Task UpdateAsync(RecurringTraining recurringTraining, CancellationToken ct = default);
}

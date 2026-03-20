using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Repositories;

public interface IRecurringTrainingRepository
{
    Task<RecurringTraining?> GetByIdAsync(RecurringTrainingId id, CancellationToken ct = default);
    Task<PagedList<RecurringTraining>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<RecurringTraining>> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(RecurringTraining recurringTraining, CancellationToken ct = default);
    Task UpdateAsync(RecurringTraining recurringTraining, CancellationToken ct = default);
}

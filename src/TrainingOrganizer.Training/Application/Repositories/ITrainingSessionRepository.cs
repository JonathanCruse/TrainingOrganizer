using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Repositories;

public interface ITrainingSessionRepository
{
    Task<TrainingSession?> GetByIdAsync(TrainingSessionId id, CancellationToken ct = default);
    Task<IReadOnlyList<TrainingSession>> GetByRecurringTrainingIdAsync(RecurringTrainingId recurringTrainingId, CancellationToken ct = default);
    Task<PagedList<TrainingSession>> GetPagedAsync(int page, int pageSize, RecurringTrainingId? recurringTrainingId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task<IReadOnlyList<TrainingSession>> GetByMemberParticipationAsync(MemberId memberId, CancellationToken ct = default);
    Task AddAsync(TrainingSession session, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TrainingSession> sessions, CancellationToken ct = default);
    Task UpdateAsync(TrainingSession session, CancellationToken ct = default);
}

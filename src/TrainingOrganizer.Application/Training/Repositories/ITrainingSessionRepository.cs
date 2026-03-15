using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Repositories;

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

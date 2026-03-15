using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Repositories;

public interface ITrainingRepository
{
    Task<Domain.Training.Training?> GetByIdAsync(TrainingId id, CancellationToken ct = default);
    Task<PagedList<Domain.Training.Training>> GetPagedAsync(int page, int pageSize, TrainingStatus? statusFilter, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task<IReadOnlyList<Domain.Training.Training>> GetByMemberParticipationAsync(MemberId memberId, CancellationToken ct = default);
    Task<IReadOnlyList<Domain.Training.Training>> GetByTrainerAsync(MemberId trainerId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task AddAsync(Domain.Training.Training training, CancellationToken ct = default);
    Task UpdateAsync(Domain.Training.Training training, CancellationToken ct = default);
}

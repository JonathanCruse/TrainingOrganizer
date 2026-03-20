using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Repositories;

public interface ITrainingRepository
{
    Task<Domain.Training?> GetByIdAsync(TrainingId id, CancellationToken ct = default);
    Task<PagedList<Domain.Training>> GetPagedAsync(int page, int pageSize, TrainingStatus? statusFilter, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task<IReadOnlyList<Domain.Training>> GetByMemberParticipationAsync(MemberId memberId, CancellationToken ct = default);
    Task<IReadOnlyList<Domain.Training>> GetByTrainerAsync(MemberId trainerId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task AddAsync(Domain.Training training, CancellationToken ct = default);
    Task UpdateAsync(Domain.Training training, CancellationToken ct = default);
}

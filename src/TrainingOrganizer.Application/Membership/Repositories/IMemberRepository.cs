using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Domain.Membership;
using TrainingOrganizer.Domain.Membership.Enums;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Application.Membership.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(MemberId id, CancellationToken ct = default);
    Task<Member?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<Member?> GetByExternalIdentityAsync(string provider, string subjectId, CancellationToken ct = default);
    Task<PagedList<Member>> GetPagedAsync(int page, int pageSize, RegistrationStatus? statusFilter, string? searchTerm, CancellationToken ct = default);
    Task<List<Member>> GetTrainersAsync(CancellationToken ct = default);
    Task AddAsync(Member member, CancellationToken ct = default);
    Task UpdateAsync(Member member, CancellationToken ct = default);
}

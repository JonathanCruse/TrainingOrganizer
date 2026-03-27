using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Domain;
using TrainingOrganizer.Membership.Domain.Enums;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Application.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(MemberId id, CancellationToken ct = default);
    Task<Member?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<Member?> GetByExternalIdentityAsync(string provider, string subjectId, CancellationToken ct = default);
    Task<PagedList<Member>> GetPagedAsync(int page, int pageSize, RegistrationStatus? statusFilter, string? searchTerm, MemberRole? roleFilter, CancellationToken ct = default);
    Task<List<Member>> GetTrainersAsync(CancellationToken ct = default);
    Task AddAsync(Member member, CancellationToken ct = default);
    Task UpdateAsync(Member member, CancellationToken ct = default);
}

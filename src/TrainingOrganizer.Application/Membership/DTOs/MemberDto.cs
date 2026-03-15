using TrainingOrganizer.Domain.Membership;
using TrainingOrganizer.Domain.Membership.Enums;

namespace TrainingOrganizer.Application.Membership.DTOs;

public sealed record MemberDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    IReadOnlySet<MemberRole> Roles,
    RegistrationStatus Status,
    DateTimeOffset RegisteredAt)
{
    public static MemberDto FromDomain(Member member) => new(
        member.Id.Value,
        member.Name.FirstName,
        member.Name.LastName,
        member.Email.Value,
        member.Phone?.Value,
        member.Roles,
        member.RegistrationStatus,
        member.RegisteredAt);
}

using TrainingOrganizer.Shared.Enums;

namespace TrainingOrganizer.Shared.Models;

// Response
public sealed record MemberResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    IReadOnlyCollection<MemberRole> Roles,
    RegistrationStatus Status,
    DateTimeOffset RegisteredAt);

// Requests
public sealed record RegisterMemberRequest(
    string FirstName,
    string LastName,
    string Email);

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone);

public sealed record RejectMemberRequest(string Reason);
public sealed record SuspendMemberRequest(string Reason);
public sealed record AssignRoleRequest(string Role);

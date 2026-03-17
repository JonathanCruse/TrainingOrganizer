namespace TrainingOrganizer.Api.Contracts;

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

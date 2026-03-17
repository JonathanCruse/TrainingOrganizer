namespace TrainingOrganizer.Application.Membership.Services;

public interface IEasyVereinApiClient
{
    Task<List<EasyVereinMemberDto>> GetAllMembersAsync(CancellationToken ct = default);
    Task<List<EasyVereinMemberGroupDto>> GetAllMemberGroupsAsync(CancellationToken ct = default);
}

public sealed record EasyVereinMemberDto
{
    public required int Id { get; init; }
    public string? EmailOrUserName { get; init; }
    public string? MembershipNumber { get; init; }
    public EasyVereinContactDetailsDto? ContactDetails { get; init; }
    public List<EasyVereinMemberGroupRefDto>? MemberGroups { get; init; }
}

public sealed record EasyVereinContactDetailsDto
{
    public int Id { get; init; }
    public string? FirstName { get; init; }
    public string? FamilyName { get; init; }
}

public sealed record EasyVereinMemberGroupRefDto
{
    public required int Id { get; init; }
    public string? Name { get; init; }
}

public sealed record EasyVereinMemberGroupDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public string? Short { get; init; }
}

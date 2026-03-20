using MediatR;
using TrainingOrganizer.Api.Contracts;
using TrainingOrganizer.Api.Extensions;
using TrainingOrganizer.Membership.Application.Commands;
using TrainingOrganizer.Membership.Application.Queries;
using TrainingOrganizer.Membership.Domain.Enums;

namespace TrainingOrganizer.Api.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/members").WithTags("Members");

        group.MapPost("/register", RegisterMember);
        group.MapGet("/me", GetCurrentMember).RequireAuthorization();
        group.MapPut("/me", UpdateProfile).RequireAuthorization();
        group.MapGet("/trainers", ListTrainers).RequireAuthorization();
        group.MapGet("/", ListMembers).RequireAuthorization("Trainer");
        group.MapGet("/{id:guid}", GetMember).RequireAuthorization("Trainer");
        group.MapPost("/{id:guid}/approve", ApproveMember).RequireAuthorization("Admin");
        group.MapPost("/{id:guid}/reject", RejectMember).RequireAuthorization("Admin");
        group.MapPost("/{id:guid}/suspend", SuspendMember).RequireAuthorization("Admin");
        group.MapPost("/{id:guid}/reinstate", ReinstateMember).RequireAuthorization("Admin");
        group.MapPost("/{id:guid}/roles", AssignRole).RequireAuthorization("Admin");
        group.MapDelete("/{id:guid}/roles/{role}", RemoveRole).RequireAuthorization("Admin");
        group.MapGet("/pending", ListPendingMembers).RequireAuthorization("Trainer");
    }

    private static async Task<IResult> RegisterMember(RegisterMemberRequest request, ISender sender)
    {
        var command = new RegisterMemberCommand(
            request.FirstName, request.LastName, request.Email);
        var result = await sender.Send(command);
        return result.ToCreatedResult($"/api/v1/members/{result.Value}");
    }

    private static async Task<IResult> GetCurrentMember(ISender sender)
    {
        var query = new GetCurrentMemberQuery();
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> UpdateProfile(UpdateProfileRequest request, ISender sender)
    {
        var command = new UpdateProfileCommand(
            request.FirstName, request.LastName, request.Email, request.Phone);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> ListMembers(
        int page, int pageSize, string? status, string? search, ISender sender)
    {
        RegistrationStatus? statusFilter = null;
        if (status is not null && Enum.TryParse<RegistrationStatus>(status, ignoreCase: true, out var parsed))
            statusFilter = parsed;

        var query = new ListMembersQuery(page, pageSize, statusFilter, search);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetMember(Guid id, ISender sender)
    {
        var query = new GetMemberQuery(id);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> ApproveMember(Guid id, ISender sender)
    {
        var command = new ApproveMemberCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> RejectMember(Guid id, RejectMemberRequest request, ISender sender)
    {
        var command = new RejectMemberCommand(id, request.Reason);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> SuspendMember(Guid id, SuspendMemberRequest request, ISender sender)
    {
        var command = new SuspendMemberCommand(id, request.Reason);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> ReinstateMember(Guid id, ISender sender)
    {
        var command = new ReinstateMemberCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> AssignRole(Guid id, AssignRoleRequest request, ISender sender)
    {
        var role = Enum.Parse<MemberRole>(request.Role, ignoreCase: true);
        var command = new AssignRoleCommand(id, role);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> RemoveRole(Guid id, string role, ISender sender)
    {
        var memberRole = Enum.Parse<MemberRole>(role, ignoreCase: true);
        var command = new RemoveRoleCommand(id, memberRole);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> ListPendingMembers(int page, int pageSize, ISender sender)
    {
        var query = new ListPendingMembersQuery(page, pageSize);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> ListTrainers(ISender sender)
    {
        var query = new ListTrainersQuery();
        var result = await sender.Send(query);
        return result.ToApiResult();
    }
}

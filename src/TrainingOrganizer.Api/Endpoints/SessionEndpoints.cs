using MediatR;
using TrainingOrganizer.Api.Contracts;
using TrainingOrganizer.Api.Extensions;
using TrainingOrganizer.Training.Application.Commands;
using TrainingOrganizer.Training.Application.Queries;
using TrainingOrganizer.Training.Domain.Enums;

namespace TrainingOrganizer.Api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/sessions")
            .WithTags("Sessions")
            .RequireAuthorization();

        group.MapGet("/", ListSessions);
        group.MapGet("/{id:guid}", GetSession);
        group.MapPost("/{id:guid}/participants", JoinSession);
        group.MapDelete("/{id:guid}/participants/me", LeaveSession);
        group.MapPost("/{id:guid}/participants/{memberId:guid}/accept", AcceptSessionParticipant);
        group.MapPost("/{id:guid}/participants/{memberId:guid}/reject", RejectSessionParticipant);
        group.MapPost("/{id:guid}/cancel", CancelSession);
        group.MapPost("/{id:guid}/complete", CompleteSession);
        group.MapPut("/{id:guid}/overrides", ApplySessionOverrides);
        group.MapPost("/{id:guid}/attendance", RecordSessionAttendance);
    }

    private static async Task<IResult> ListSessions(
        int page, int pageSize, Guid? recurringTrainingId, string? status, DateTimeOffset? from, DateTimeOffset? to, ISender sender)
    {
        SessionStatus? statusFilter = null;
        if (status is not null && Enum.TryParse<SessionStatus>(status, ignoreCase: true, out var parsed))
            statusFilter = parsed;

        var query = new ListSessionsQuery(page, pageSize, recurringTrainingId, statusFilter, from, to);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetSession(Guid id, ISender sender)
    {
        var query = new GetSessionQuery(id);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> JoinSession(Guid id, ISender sender)
    {
        var command = new JoinSessionCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> LeaveSession(Guid id, ISender sender)
    {
        var command = new LeaveSessionCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> AcceptSessionParticipant(Guid id, Guid memberId, ISender sender)
    {
        var command = new AcceptSessionParticipantCommand(id, memberId);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> RejectSessionParticipant(Guid id, Guid memberId, ISender sender)
    {
        var command = new RejectSessionParticipantCommand(id, memberId);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> CancelSession(Guid id, CancelSessionRequest request, ISender sender)
    {
        var command = new CancelSessionCommand(id, request.Reason);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> CompleteSession(Guid id, ISender sender)
    {
        var command = new CompleteSessionCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> ApplySessionOverrides(Guid id, ApplySessionOverridesRequest request, ISender sender)
    {
        Visibility? visibility = null;
        if (request.Visibility is not null)
            visibility = Enum.Parse<Visibility>(request.Visibility, ignoreCase: true);

        var command = new ApplySessionOverridesCommand(
            id, request.Title, request.Description,
            request.MinCapacity, request.MaxCapacity, visibility);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> RecordSessionAttendance(
        Guid id, RecordSessionAttendanceRequest request, ISender sender)
    {
        var entries = request.Entries
            .Select(e => new AttendanceEntry(e.MemberId, e.Attended))
            .ToList();
        var command = new RecordSessionAttendanceCommand(id, entries);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }
}

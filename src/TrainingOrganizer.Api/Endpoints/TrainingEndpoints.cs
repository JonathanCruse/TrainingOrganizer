using MediatR;
using TrainingOrganizer.Api.Contracts;
using TrainingOrganizer.Api.Extensions;
using TrainingOrganizer.Training.Application.Commands;
using TrainingOrganizer.Training.Application.Queries;
using TrainingOrganizer.Training.Domain.Enums;

namespace TrainingOrganizer.Api.Endpoints;

public static class TrainingEndpoints
{
    public static void MapTrainingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/trainings").WithTags("Trainings").RequireAuthorization();

        group.MapPost("/", CreateTraining);
        group.MapGet("/", ListTrainings);
        group.MapGet("/{id:guid}", GetTraining);
        group.MapPut("/{id:guid}", UpdateTraining);
        group.MapPost("/{id:guid}/publish", PublishTraining);
        group.MapPost("/{id:guid}/cancel", CancelTraining);
        group.MapPost("/{id:guid}/complete", CompleteTraining);
        group.MapPost("/{id:guid}/participants", JoinTraining);
        group.MapDelete("/{id:guid}/participants/me", LeaveTraining);
        group.MapPost("/{id:guid}/participants/{memberId:guid}/accept", AcceptParticipant);
        group.MapPost("/{id:guid}/participants/{memberId:guid}/reject", RejectParticipant);
        group.MapPost("/{id:guid}/attendance", RecordAttendance);
        group.MapPost("/{id:guid}/trainers", AssignTrainer);
        group.MapDelete("/{id:guid}/trainers/{trainerId:guid}", RemoveTrainer);
    }

    private static async Task<IResult> CreateTraining(CreateTrainingRequest request, ISender sender)
    {
        var visibility = Enum.Parse<Visibility>(request.Visibility, ignoreCase: true);
        var command = new CreateTrainingCommand(
            request.Title, request.Description, request.Start, request.End,
            request.MinCapacity, request.MaxCapacity, visibility, request.TrainerIds);
        var result = await sender.Send(command);
        return result.ToCreatedResult($"/api/v1/trainings/{result.Value}");
    }

    private static async Task<IResult> ListTrainings(
        int page, int pageSize, string? status, DateTimeOffset? from, DateTimeOffset? to, ISender sender)
    {
        TrainingStatus? statusFilter = null;
        if (status is not null && Enum.TryParse<TrainingStatus>(status, ignoreCase: true, out var parsed))
            statusFilter = parsed;

        var query = new ListTrainingsQuery(page, pageSize, statusFilter, from, to);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetTraining(Guid id, ISender sender)
    {
        var query = new GetTrainingQuery(id);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> UpdateTraining(Guid id, UpdateTrainingRequest request, ISender sender)
    {
        var visibility = Enum.Parse<Visibility>(request.Visibility, ignoreCase: true);
        var command = new UpdateTrainingCommand(
            id, request.Title, request.Description, request.Start, request.End,
            request.MinCapacity, request.MaxCapacity, visibility);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> PublishTraining(Guid id, ISender sender)
    {
        var command = new PublishTrainingCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> CancelTraining(Guid id, CancelTrainingRequest request, ISender sender)
    {
        var command = new CancelTrainingCommand(id, request.Reason);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> CompleteTraining(Guid id, ISender sender)
    {
        var command = new CompleteTrainingCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> JoinTraining(Guid id, ISender sender)
    {
        var command = new JoinTrainingCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> LeaveTraining(Guid id, ISender sender)
    {
        var command = new LeaveTrainingCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> AcceptParticipant(Guid id, Guid memberId, ISender sender)
    {
        var command = new AcceptTrainingParticipantCommand(id, memberId);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> RejectParticipant(Guid id, Guid memberId, ISender sender)
    {
        var command = new RejectTrainingParticipantCommand(id, memberId);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> RecordAttendance(Guid id, RecordAttendanceRequest request, ISender sender)
    {
        var entries = request.Entries
            .Select(e => new AttendanceEntry(e.MemberId, e.Attended))
            .ToList();
        var command = new RecordTrainingAttendanceCommand(id, entries);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> AssignTrainer(Guid id, AssignTrainerRequest request, ISender sender)
    {
        var command = new AssignTrainerCommand(id, request.TrainerId);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> RemoveTrainer(Guid id, Guid trainerId, ISender sender)
    {
        var command = new RemoveTrainerCommand(id, trainerId);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }
}

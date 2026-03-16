using MediatR;
using TrainingOrganizer.Api.Contracts;
using TrainingOrganizer.Api.Extensions;
using TrainingOrganizer.Application.Training.Commands;
using TrainingOrganizer.Application.Training.DTOs;
using TrainingOrganizer.Application.Training.Queries;
using TrainingOrganizer.Domain.Training.Enums;

namespace TrainingOrganizer.Api.Endpoints;

public static class RecurringTrainingEndpoints
{
    public static void MapRecurringTrainingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/recurring-trainings")
            .WithTags("RecurringTrainings")
            .RequireAuthorization();

        group.MapPost("/", CreateRecurringTraining);
        group.MapGet("/", ListRecurringTrainings);
        group.MapGet("/{id:guid}", GetRecurringTraining);
        group.MapPost("/{id:guid}/pause", PauseRecurringTraining);
        group.MapPost("/{id:guid}/resume", ResumeRecurringTraining);
        group.MapPost("/{id:guid}/end", EndRecurringTraining);
        group.MapPost("/{id:guid}/generate", GenerateSessions);
    }

    private static async Task<IResult> CreateRecurringTraining(
        CreateRecurringTrainingRequest request, ISender sender)
    {
        var visibility = Enum.Parse<Visibility>(request.Visibility, ignoreCase: true);
        var pattern = Enum.Parse<RecurrencePattern>(request.Pattern, ignoreCase: true);
        var command = new CreateRecurringTrainingCommand(
            request.Title, request.Description,
            request.MinCapacity, request.MaxCapacity,
            visibility, request.TrainerIds, [],
            pattern, request.DayOfWeek,
            request.TimeOfDay, request.Duration,
            request.StartDate, request.EndDate);
        var result = await sender.Send(command);
        return result.ToCreatedResult($"/api/v1/recurring-trainings/{result.Value}");
    }

    private static async Task<IResult> ListRecurringTrainings(int page, int pageSize, ISender sender)
    {
        var query = new ListRecurringTrainingsQuery(page, pageSize);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetRecurringTraining(Guid id, ISender sender)
    {
        var query = new GetRecurringTrainingQuery(id);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> PauseRecurringTraining(Guid id, ISender sender)
    {
        var command = new PauseRecurringTrainingCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> ResumeRecurringTraining(Guid id, ISender sender)
    {
        var command = new ResumeRecurringTrainingCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> EndRecurringTraining(Guid id, ISender sender)
    {
        var command = new EndRecurringTrainingCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> GenerateSessions(Guid id, GenerateSessionsRequest request, ISender sender)
    {
        var command = new GenerateSessionsCommand(id, request.Until.ToString("yyyy-MM-dd"));
        var result = await sender.Send(command);
        return result.ToApiResult();
    }
}

using MediatR;
using TrainingOrganizer.Api.Extensions;
using TrainingOrganizer.Training.Application.Schedule.Queries;

namespace TrainingOrganizer.Api.Endpoints;

public static class ScheduleEndpoints
{
    public static void MapScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/schedule")
            .WithTags("Schedule")
            .RequireAuthorization();

        group.MapGet("/me", GetPersonalSchedule);
        group.MapGet("/trainers/{id:guid}", GetTrainerSchedule);
    }

    private static async Task<IResult> GetPersonalSchedule(
        DateTimeOffset from, DateTimeOffset to, ISender sender)
    {
        var query = new GetPersonalScheduleQuery(from, to);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetTrainerSchedule(
        Guid id, DateTimeOffset from, DateTimeOffset to, ISender sender)
    {
        var query = new GetTrainerScheduleQuery(id, from, to);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }
}

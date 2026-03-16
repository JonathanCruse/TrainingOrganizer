using MediatR;
using TrainingOrganizer.Api.Contracts;
using TrainingOrganizer.Api.Extensions;
using TrainingOrganizer.Application.Facility.Commands;
using TrainingOrganizer.Application.Facility.Queries;

namespace TrainingOrganizer.Api.Endpoints;

public static class LocationEndpoints
{
    public static void MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/locations")
            .WithTags("Locations")
            .RequireAuthorization();

        group.MapPost("/", CreateLocation);
        group.MapGet("/", ListLocations);
        group.MapGet("/{id:guid}", GetLocation);
        group.MapPut("/{id:guid}", UpdateLocation);
        group.MapPost("/{id:guid}/rooms", AddRoom);
        group.MapPut("/{id:guid}/rooms/{roomId:guid}", UpdateRoom);
        group.MapPost("/{id:guid}/rooms/{roomId:guid}/enable", EnableRoom);
        group.MapPost("/{id:guid}/rooms/{roomId:guid}/disable", DisableRoom);
    }

    private static async Task<IResult> CreateLocation(CreateLocationRequest request, ISender sender)
    {
        var command = new CreateLocationCommand(
            request.Name, request.Street, request.City, request.PostalCode, request.Country);
        var result = await sender.Send(command);
        return result.ToCreatedResult($"/api/v1/locations/{result.Value}");
    }

    private static async Task<IResult> ListLocations(ISender sender)
    {
        var query = new ListLocationsQuery();
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetLocation(Guid id, ISender sender)
    {
        var query = new GetLocationQuery(id);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> UpdateLocation(Guid id, UpdateLocationRequest request, ISender sender)
    {
        var command = new UpdateLocationCommand(
            id, request.Name, request.Street, request.City, request.PostalCode, request.Country);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> AddRoom(Guid id, AddRoomRequest request, ISender sender)
    {
        var command = new AddRoomCommand(id, request.Name, request.Capacity);
        var result = await sender.Send(command);
        return result.ToCreatedResult($"/api/v1/locations/{id}/rooms/{result.Value}");
    }

    private static async Task<IResult> UpdateRoom(Guid id, Guid roomId, UpdateRoomRequest request, ISender sender)
    {
        var command = new UpdateRoomCommand(id, roomId, request.Name, request.Capacity);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> EnableRoom(Guid id, Guid roomId, ISender sender)
    {
        var command = new EnableRoomCommand(id, roomId);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> DisableRoom(Guid id, Guid roomId, ISender sender)
    {
        var command = new DisableRoomCommand(id, roomId);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }
}

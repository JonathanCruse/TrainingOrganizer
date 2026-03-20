using MediatR;
using TrainingOrganizer.Api.Contracts;
using TrainingOrganizer.Api.Extensions;
using TrainingOrganizer.Facility.Application.Commands;
using TrainingOrganizer.Facility.Application.Queries;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Api.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/bookings")
            .WithTags("Bookings")
            .RequireAuthorization();

        group.MapPost("/", CreateBooking);
        group.MapGet("/", ListBookings);
        group.MapPost("/{id:guid}/cancel", CancelBooking);
        group.MapPost("/{id:guid}/reschedule", RescheduleBooking);
        group.MapGet("/rooms/{roomId:guid}/availability", GetRoomAvailability);
    }

    private static async Task<IResult> CreateBooking(CreateBookingRequest request, ISender sender)
    {
        var referenceType = Enum.Parse<BookingReferenceType>(request.ReferenceType, ignoreCase: true);
        var command = new CreateBookingCommand(
            request.RoomId, request.LocationId, request.Start, request.End,
            referenceType, request.ReferenceId);
        var result = await sender.Send(command);
        return result.ToCreatedResult($"/api/v1/bookings/{result.Value}");
    }

    private static async Task<IResult> ListBookings(
        Guid? roomId, DateTimeOffset? from, DateTimeOffset? to, int page, int pageSize, ISender sender)
    {
        var query = new ListBookingsQuery(roomId, from, to, page, pageSize);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }

    private static async Task<IResult> CancelBooking(Guid id, ISender sender)
    {
        var command = new CancelBookingCommand(id);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> RescheduleBooking(Guid id, RescheduleBookingRequest request, ISender sender)
    {
        var command = new RescheduleBookingCommand(id, request.Start, request.End);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetRoomAvailability(
        Guid roomId, DateTimeOffset from, DateTimeOffset to, ISender sender)
    {
        var query = new GetRoomAvailabilityQuery(roomId, from, to);
        var result = await sender.Send(query);
        return result.ToApiResult();
    }
}

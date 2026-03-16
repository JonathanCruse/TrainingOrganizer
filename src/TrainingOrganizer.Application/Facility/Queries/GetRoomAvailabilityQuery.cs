using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Facility.DTOs;
using TrainingOrganizer.Application.Facility.Repositories;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Application.Facility.Queries;

public sealed record GetRoomAvailabilityQuery(
    Guid RoomId,
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<Result<IReadOnlyList<TimeSlotDto>>>;

public sealed class GetRoomAvailabilityQueryHandler : IRequestHandler<GetRoomAvailabilityQuery, Result<IReadOnlyList<TimeSlotDto>>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetRoomAvailabilityQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<IReadOnlyList<TimeSlotDto>>> Handle(GetRoomAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var roomId = new RoomId(request.RoomId);

        var bookings = await _bookingRepository.GetByRoomAndDateRangeAsync(
            roomId, request.From, request.To, cancellationToken);

        var bookedSlots = bookings
            .Where(b => b.IsActive)
            .Select(b => TimeSlotDto.FromDomain(b.TimeSlot))
            .OrderBy(s => s.Start)
            .ToList();

        return Result.Success<IReadOnlyList<TimeSlotDto>>(bookedSlots);
    }
}

public sealed class GetRoomAvailabilityQueryValidator : AbstractValidator<GetRoomAvailabilityQuery>
{
    public GetRoomAvailabilityQueryValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty().GreaterThan(x => x.From)
            .WithMessage("To must be after From.");
    }
}

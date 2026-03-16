using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Facility.Repositories;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.ValueObjects;
using TrainingOrganizer.Domain.Services;

namespace TrainingOrganizer.Application.Facility.Commands;

public sealed record RescheduleBookingCommand(
    Guid BookingId,
    DateTimeOffset Start,
    DateTimeOffset End) : IRequest<Result>;

public sealed class RescheduleBookingCommandHandler : IRequestHandler<RescheduleBookingCommand, Result>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomBookingService _roomBookingService;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleBookingCommandHandler(
        IBookingRepository bookingRepository,
        IRoomBookingService roomBookingService,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _roomBookingService = roomBookingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RescheduleBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var bookingId = new BookingId(request.BookingId);
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken)
                ?? throw new NotFoundException(nameof(Booking), request.BookingId);

            var newTimeSlot = new TimeSlot(request.Start, request.End);

            var hasConflict = await _roomBookingService.HasConflictAsync(
                booking.RoomId, newTimeSlot, bookingId, cancellationToken);
            if (hasConflict)
                return Result.Failure("Booking.Conflict", "The room is already booked for the requested time slot.");

            booking.Reschedule(newTimeSlot);

            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure("Booking.DomainError", ex.Message);
        }
    }
}

public sealed class RescheduleBookingCommandValidator : AbstractValidator<RescheduleBookingCommand>
{
    public RescheduleBookingCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
        RuleFor(x => x.Start).NotEmpty();
        RuleFor(x => x.End).NotEmpty().GreaterThan(x => x.Start)
            .WithMessage("End must be after Start.");
    }
}

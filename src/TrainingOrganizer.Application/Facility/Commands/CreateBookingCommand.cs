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

public sealed record CreateBookingCommand(
    Guid RoomId,
    Guid LocationId,
    DateTimeOffset Start,
    DateTimeOffset End,
    BookingReferenceType ReferenceType,
    Guid ReferenceId) : IRequest<Result<Guid>>;

public sealed class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<Guid>>
{
    private readonly IRoomBookingService _roomBookingService;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(
        IRoomBookingService roomBookingService,
        IBookingRepository bookingRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _roomBookingService = roomBookingService;
        _bookingRepository = bookingRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.MemberId
                ?? throw new ForbiddenException("You must be authenticated to create a booking.");

            var roomId = new RoomId(request.RoomId);
            var locationId = new LocationId(request.LocationId);
            var timeSlot = new TimeSlot(request.Start, request.End);
            var reference = new BookingReference(request.ReferenceType, request.ReferenceId);

            var hasConflict = await _roomBookingService.HasConflictAsync(roomId, timeSlot, cancellationToken: cancellationToken);
            if (hasConflict)
                return Result.Failure<Guid>("Booking.Conflict", "The room is already booked for the requested time slot.");

            var booking = Booking.Create(roomId, locationId, timeSlot, reference, currentUserId.Value);

            await _bookingRepository.AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(booking.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>("Booking.DomainError", ex.Message);
        }
    }
}

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.Start).NotEmpty();
        RuleFor(x => x.End).NotEmpty().GreaterThan(x => x.Start)
            .WithMessage("End must be after Start.");
        RuleFor(x => x.ReferenceType).IsInEnum();
        RuleFor(x => x.ReferenceId).NotEmpty();
    }
}

using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Facility.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Application.Facility.Commands;

public sealed record EnableRoomCommand(Guid LocationId, Guid RoomId) : IRequest<Result>;

public sealed class EnableRoomCommandHandler : IRequestHandler<EnableRoomCommand, Result>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnableRoomCommandHandler(
        ILocationRepository locationRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(EnableRoomCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var locationId = new LocationId(request.LocationId);
            var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken)
                ?? throw new NotFoundException(nameof(Location), request.LocationId);

            location.EnableRoom(new RoomId(request.RoomId));

            await _locationRepository.UpdateAsync(location, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure("Room.DomainError", ex.Message);
        }
    }
}

public sealed class EnableRoomCommandValidator : AbstractValidator<EnableRoomCommand>
{
    public EnableRoomCommandValidator()
    {
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
    }
}

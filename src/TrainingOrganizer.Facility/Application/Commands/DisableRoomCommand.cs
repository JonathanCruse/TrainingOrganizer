using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Application.Commands;

public sealed record DisableRoomCommand(Guid LocationId, Guid RoomId) : IRequest<Result>;

public sealed class DisableRoomCommandHandler : IRequestHandler<DisableRoomCommand, Result>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DisableRoomCommandHandler(
        ILocationRepository locationRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DisableRoomCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var locationId = new LocationId(request.LocationId);
            var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken)
                ?? throw new NotFoundException(nameof(Location), request.LocationId);

            location.DisableRoom(new RoomId(request.RoomId));

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

public sealed class DisableRoomCommandValidator : AbstractValidator<DisableRoomCommand>
{
    public DisableRoomCommandValidator()
    {
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
    }
}

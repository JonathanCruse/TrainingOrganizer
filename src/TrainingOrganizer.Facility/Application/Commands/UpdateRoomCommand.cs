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

public sealed record UpdateRoomCommand(
    Guid LocationId,
    Guid RoomId,
    string Name,
    int Capacity) : IRequest<Result>;

public sealed class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, Result>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoomCommandHandler(
        ILocationRepository locationRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var locationId = new LocationId(request.LocationId);
            var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken)
                ?? throw new NotFoundException(nameof(Location), request.LocationId);

            location.UpdateRoom(new RoomId(request.RoomId), new RoomName(request.Name), request.Capacity);

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

public sealed class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}

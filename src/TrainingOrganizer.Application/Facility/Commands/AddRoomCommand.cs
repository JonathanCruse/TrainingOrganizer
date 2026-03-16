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

public sealed record AddRoomCommand(Guid LocationId, string Name, int Capacity) : IRequest<Result<Guid>>;

public sealed class AddRoomCommandHandler : IRequestHandler<AddRoomCommand, Result<Guid>>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddRoomCommandHandler(
        ILocationRepository locationRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddRoomCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var locationId = new LocationId(request.LocationId);
            var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken)
                ?? throw new NotFoundException(nameof(Location), request.LocationId);

            var room = location.AddRoom(new RoomName(request.Name), request.Capacity);

            await _locationRepository.UpdateAsync(location, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(room.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>("Room.DomainError", ex.Message);
        }
    }
}

public sealed class AddRoomCommandValidator : AbstractValidator<AddRoomCommand>
{
    public AddRoomCommandValidator()
    {
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}

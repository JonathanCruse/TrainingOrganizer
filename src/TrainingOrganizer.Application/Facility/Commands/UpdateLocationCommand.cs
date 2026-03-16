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

public sealed record UpdateLocationCommand(
    Guid LocationId,
    string Name,
    string Street,
    string City,
    string PostalCode,
    string Country) : IRequest<Result>;

public sealed class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, Result>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLocationCommandHandler(
        ILocationRepository locationRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var locationId = new LocationId(request.LocationId);
            var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken)
                ?? throw new NotFoundException(nameof(Location), request.LocationId);

            location.UpdateName(new LocationName(request.Name));
            location.UpdateAddress(new Address(request.Street, request.City, request.PostalCode, request.Country));

            await _locationRepository.UpdateAsync(location, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure("Location.DomainError", ex.Message);
        }
    }
}

public sealed class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationCommandValidator()
    {
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

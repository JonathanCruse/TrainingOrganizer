using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Application.DTOs;
using TrainingOrganizer.Facility.Application.Repositories;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Application.Queries;

public sealed record GetLocationQuery(Guid LocationId) : IRequest<Result<LocationDto>>;

public sealed class GetLocationQueryHandler : IRequestHandler<GetLocationQuery, Result<LocationDto>>
{
    private readonly ILocationRepository _locationRepository;

    public GetLocationQueryHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<LocationDto>> Handle(GetLocationQuery request, CancellationToken cancellationToken)
    {
        var locationId = new LocationId(request.LocationId);
        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        return Result.Success(LocationDto.FromDomain(location));
    }
}

public sealed class GetLocationQueryValidator : AbstractValidator<GetLocationQuery>
{
    public GetLocationQueryValidator()
    {
        RuleFor(x => x.LocationId).NotEmpty();
    }
}

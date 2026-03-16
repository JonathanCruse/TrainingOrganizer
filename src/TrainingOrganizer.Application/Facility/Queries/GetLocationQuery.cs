using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Facility.DTOs;
using TrainingOrganizer.Application.Facility.Repositories;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Application.Facility.Queries;

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

using MediatR;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Facility.DTOs;
using TrainingOrganizer.Application.Facility.Repositories;

namespace TrainingOrganizer.Application.Facility.Queries;

public sealed record ListLocationsQuery() : IRequest<Result<IReadOnlyList<LocationDto>>>;

public sealed class ListLocationsQueryHandler : IRequestHandler<ListLocationsQuery, Result<IReadOnlyList<LocationDto>>>
{
    private readonly ILocationRepository _locationRepository;

    public ListLocationsQueryHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<IReadOnlyList<LocationDto>>> Handle(ListLocationsQuery request, CancellationToken cancellationToken)
    {
        var locations = await _locationRepository.GetAllAsync(cancellationToken);

        var dtos = locations.Select(LocationDto.FromDomain).ToList();

        return Result.Success<IReadOnlyList<LocationDto>>(dtos);
    }
}

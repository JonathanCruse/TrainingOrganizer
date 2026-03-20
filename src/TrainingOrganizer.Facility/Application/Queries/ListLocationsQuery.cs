using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Application.DTOs;
using TrainingOrganizer.Facility.Application.Repositories;

namespace TrainingOrganizer.Facility.Application.Queries;

public sealed record ListLocationsQuery(int Page, int PageSize) : IRequest<Result<PagedList<LocationDto>>>;

public sealed class ListLocationsQueryHandler : IRequestHandler<ListLocationsQuery, Result<PagedList<LocationDto>>>
{
    private readonly ILocationRepository _locationRepository;

    public ListLocationsQueryHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<PagedList<LocationDto>>> Handle(ListLocationsQuery request, CancellationToken cancellationToken)
    {
        var pagedLocations = await _locationRepository.GetPagedAsync(
            request.Page, request.PageSize, cancellationToken);

        var dtos = pagedLocations.Items.Select(LocationDto.FromDomain).ToList();

        return Result.Success(new PagedList<LocationDto>(
            dtos, pagedLocations.Page, pagedLocations.PageSize, pagedLocations.TotalCount));
    }
}

public sealed class ListLocationsQueryValidator : AbstractValidator<ListLocationsQuery>
{
    public ListLocationsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Application.DTOs;
using TrainingOrganizer.Facility.Application.Repositories;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Application.Queries;

public sealed record ListBookingsQuery(
    Guid? RoomId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize) : IRequest<Result<PagedList<BookingDto>>>;

public sealed class ListBookingsQueryHandler : IRequestHandler<ListBookingsQuery, Result<PagedList<BookingDto>>>
{
    private readonly IBookingRepository _bookingRepository;

    public ListBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<PagedList<BookingDto>>> Handle(ListBookingsQuery request, CancellationToken cancellationToken)
    {
        var roomId = request.RoomId.HasValue ? new RoomId(request.RoomId.Value) : null;

        var paged = await _bookingRepository.GetPagedAsync(
            request.Page, request.PageSize, roomId, request.From, request.To, cancellationToken);

        var dtos = paged.Items.Select(BookingDto.FromDomain).ToList();

        return Result.Success(new PagedList<BookingDto>(
            dtos, paged.Page, paged.PageSize, paged.TotalCount));
    }
}

public sealed class ListBookingsQueryValidator : AbstractValidator<ListBookingsQuery>
{
    public ListBookingsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.To).GreaterThan(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("To must be after From.");
    }
}

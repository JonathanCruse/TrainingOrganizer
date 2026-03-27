using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.DTOs;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Queries;

public sealed record ListSessionsQuery(
    int Page,
    int PageSize,
    Guid? RecurringTrainingId,
    SessionStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To) : IRequest<Result<PagedList<TrainingSessionDto>>>;

public sealed class ListSessionsQueryHandler : IRequestHandler<ListSessionsQuery, Result<PagedList<TrainingSessionDto>>>
{
    private readonly ITrainingSessionRepository _sessionRepository;

    public ListSessionsQueryHandler(ITrainingSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<PagedList<TrainingSessionDto>>> Handle(ListSessionsQuery request, CancellationToken cancellationToken)
    {
        var recurringTrainingId = request.RecurringTrainingId.HasValue
            ? new RecurringTrainingId(request.RecurringTrainingId.Value)
            : null;

        var paged = await _sessionRepository.GetPagedAsync(
            request.Page, request.PageSize, recurringTrainingId, request.Status, request.From, request.To, cancellationToken);

        var dtos = paged.Items.Select(TrainingSessionDto.FromDomain).ToList();

        return Result.Success(new PagedList<TrainingSessionDto>(
            dtos, paged.Page, paged.PageSize, paged.TotalCount));
    }
}

public sealed class ListSessionsQueryValidator : AbstractValidator<ListSessionsQuery>
{
    public ListSessionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.To).GreaterThan(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("To must be after From.");
    }
}

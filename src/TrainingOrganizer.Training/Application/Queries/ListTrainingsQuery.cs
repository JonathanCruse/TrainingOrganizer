using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.DTOs;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Training.Domain.Enums;

namespace TrainingOrganizer.Training.Application.Queries;

public sealed record ListTrainingsQuery(
    int Page,
    int PageSize,
    TrainingStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To) : IRequest<Result<PagedList<TrainingDto>>>;

public sealed class ListTrainingsQueryHandler : IRequestHandler<ListTrainingsQuery, Result<PagedList<TrainingDto>>>
{
    private readonly ITrainingRepository _trainingRepository;

    public ListTrainingsQueryHandler(ITrainingRepository trainingRepository)
    {
        _trainingRepository = trainingRepository;
    }

    public async Task<Result<PagedList<TrainingDto>>> Handle(ListTrainingsQuery request, CancellationToken cancellationToken)
    {
        var pagedTrainings = await _trainingRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Status, request.From, request.To, cancellationToken);

        var dtos = pagedTrainings.Items.Select(TrainingDto.FromDomain).ToList();

        return Result.Success(new PagedList<TrainingDto>(
            dtos, pagedTrainings.Page, pagedTrainings.PageSize, pagedTrainings.TotalCount));
    }
}

public sealed class ListTrainingsQueryValidator : AbstractValidator<ListTrainingsQuery>
{
    public ListTrainingsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.To).GreaterThan(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("To must be after From.");
    }
}

using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.DTOs;
using TrainingOrganizer.Training.Application.Repositories;

namespace TrainingOrganizer.Training.Application.Queries;

public sealed record ListRecurringTrainingsQuery(int Page, int PageSize) : IRequest<Result<PagedList<RecurringTrainingDto>>>;

public sealed class ListRecurringTrainingsQueryHandler : IRequestHandler<ListRecurringTrainingsQuery, Result<PagedList<RecurringTrainingDto>>>
{
    private readonly IRecurringTrainingRepository _recurringTrainingRepository;

    public ListRecurringTrainingsQueryHandler(IRecurringTrainingRepository recurringTrainingRepository)
    {
        _recurringTrainingRepository = recurringTrainingRepository;
    }

    public async Task<Result<PagedList<RecurringTrainingDto>>> Handle(ListRecurringTrainingsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _recurringTrainingRepository.GetPagedAsync(
            request.Page, request.PageSize, cancellationToken);

        var dtos = paged.Items.Select(RecurringTrainingDto.FromDomain).ToList();

        return Result.Success(new PagedList<RecurringTrainingDto>(
            dtos, paged.Page, paged.PageSize, paged.TotalCount));
    }
}

public sealed class ListRecurringTrainingsQueryValidator : AbstractValidator<ListRecurringTrainingsQuery>
{
    public ListRecurringTrainingsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

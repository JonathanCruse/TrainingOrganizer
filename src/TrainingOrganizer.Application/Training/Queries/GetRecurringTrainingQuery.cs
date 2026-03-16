using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.DTOs;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Queries;

public sealed record GetRecurringTrainingQuery(Guid Id) : IRequest<Result<RecurringTrainingDto>>;

public sealed class GetRecurringTrainingQueryHandler : IRequestHandler<GetRecurringTrainingQuery, Result<RecurringTrainingDto>>
{
    private readonly IRecurringTrainingRepository _recurringTrainingRepository;

    public GetRecurringTrainingQueryHandler(IRecurringTrainingRepository recurringTrainingRepository)
    {
        _recurringTrainingRepository = recurringTrainingRepository;
    }

    public async Task<Result<RecurringTrainingDto>> Handle(GetRecurringTrainingQuery request, CancellationToken cancellationToken)
    {
        var id = new RecurringTrainingId(request.Id);
        var recurringTraining = await _recurringTrainingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(RecurringTraining), request.Id);

        return Result.Success(RecurringTrainingDto.FromDomain(recurringTraining));
    }
}

public sealed class GetRecurringTrainingQueryValidator : AbstractValidator<GetRecurringTrainingQuery>
{
    public GetRecurringTrainingQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

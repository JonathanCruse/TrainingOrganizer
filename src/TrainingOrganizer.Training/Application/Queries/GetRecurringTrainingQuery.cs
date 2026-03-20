using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.DTOs;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Queries;

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

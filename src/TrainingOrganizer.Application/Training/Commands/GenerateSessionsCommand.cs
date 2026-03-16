using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Commands;

public sealed record GenerateSessionsCommand(Guid RecurringTrainingId, string Until) : IRequest<Result>;

public sealed class GenerateSessionsCommandHandler : IRequestHandler<GenerateSessionsCommand, Result>
{
    private readonly IRecurringTrainingRepository _recurringTrainingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateSessionsCommandHandler(
        IRecurringTrainingRepository recurringTrainingRepository,
        IUnitOfWork unitOfWork)
    {
        _recurringTrainingRepository = recurringTrainingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(GenerateSessionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = new RecurringTrainingId(request.RecurringTrainingId);
            var recurringTraining = await _recurringTrainingRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(nameof(RecurringTraining), request.RecurringTrainingId);

            var until = DateOnly.Parse(request.Until);
            recurringTraining.GenerateSessionsUntil(until);

            await _recurringTrainingRepository.UpdateAsync(recurringTraining, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure("RecurringTraining.DomainError", ex.Message);
        }
    }
}

public sealed class GenerateSessionsCommandValidator : AbstractValidator<GenerateSessionsCommand>
{
    public GenerateSessionsCommandValidator()
    {
        RuleFor(x => x.RecurringTrainingId).NotEmpty();
        RuleFor(x => x.Until).NotEmpty();
    }
}

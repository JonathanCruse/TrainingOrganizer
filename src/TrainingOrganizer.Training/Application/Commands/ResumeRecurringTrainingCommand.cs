using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Commands;

public sealed record ResumeRecurringTrainingCommand(Guid RecurringTrainingId) : IRequest<Result>;

public sealed class ResumeRecurringTrainingCommandHandler : IRequestHandler<ResumeRecurringTrainingCommand, Result>
{
    private readonly IRecurringTrainingRepository _recurringTrainingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResumeRecurringTrainingCommandHandler(
        IRecurringTrainingRepository recurringTrainingRepository,
        IUnitOfWork unitOfWork)
    {
        _recurringTrainingRepository = recurringTrainingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResumeRecurringTrainingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = new RecurringTrainingId(request.RecurringTrainingId);
            var recurringTraining = await _recurringTrainingRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(nameof(RecurringTraining), request.RecurringTrainingId);

            recurringTraining.Resume();

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

public sealed class ResumeRecurringTrainingCommandValidator : AbstractValidator<ResumeRecurringTrainingCommand>
{
    public ResumeRecurringTrainingCommandValidator()
    {
        RuleFor(x => x.RecurringTrainingId).NotEmpty();
    }
}

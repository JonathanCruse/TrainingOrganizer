using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Commands;

public sealed record UpdateTrainingCommand(
    Guid TrainingId,
    string Title,
    string? Description,
    DateTimeOffset Start,
    DateTimeOffset End,
    int MinCapacity,
    int MaxCapacity,
    Visibility Visibility) : IRequest<Result>;

public sealed class UpdateTrainingCommandHandler : IRequestHandler<UpdateTrainingCommand, Result>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTrainingCommandHandler(
        ITrainingRepository trainingRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTrainingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var trainingId = new TrainingId(request.TrainingId);
            var training = await _trainingRepository.GetByIdAsync(trainingId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Training), request.TrainingId);

            var title = new TrainingTitle(request.Title);
            var description = new TrainingDescription(request.Description ?? string.Empty);
            var timeSlot = new TimeSlot(request.Start, request.End);
            var capacity = new Capacity(request.MinCapacity, request.MaxCapacity);

            training.Update(title, description, timeSlot, capacity, request.Visibility);

            await _trainingRepository.UpdateAsync(training, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure("Training.DomainError", ex.Message);
        }
    }
}

public sealed class UpdateTrainingCommandValidator : AbstractValidator<UpdateTrainingCommand>
{
    public UpdateTrainingCommandValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000).When(x => x.Description is not null);
        RuleFor(x => x.Start).NotEmpty();
        RuleFor(x => x.End).NotEmpty().GreaterThan(x => x.Start)
            .WithMessage("End must be after Start.");
        RuleFor(x => x.MinCapacity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxCapacity).GreaterThan(0)
            .GreaterThanOrEqualTo(x => x.MinCapacity)
            .WithMessage("MaxCapacity must be greater than or equal to MinCapacity.");
        RuleFor(x => x.Visibility).IsInEnum();
    }
}

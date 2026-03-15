using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Commands;

public sealed record PublishTrainingCommand(Guid TrainingId) : IRequest<Result>;

public sealed class PublishTrainingCommandHandler : IRequestHandler<PublishTrainingCommand, Result>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishTrainingCommandHandler(
        ITrainingRepository trainingRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(PublishTrainingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var trainingId = new TrainingId(request.TrainingId);
            var training = await _trainingRepository.GetByIdAsync(trainingId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Training.Training), request.TrainingId);

            training.Publish();

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

public sealed class PublishTrainingCommandValidator : AbstractValidator<PublishTrainingCommand>
{
    public PublishTrainingCommandValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
    }
}

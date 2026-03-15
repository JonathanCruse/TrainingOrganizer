using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Commands;

public sealed record LeaveTrainingCommand(Guid TrainingId) : IRequest<Result>;

public sealed class LeaveTrainingCommandHandler : IRequestHandler<LeaveTrainingCommand, Result>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveTrainingCommandHandler(
        ITrainingRepository trainingRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LeaveTrainingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.MemberId
                ?? throw new ForbiddenException("You must be authenticated to leave a training.");

            var trainingId = new TrainingId(request.TrainingId);
            var training = await _trainingRepository.GetByIdAsync(trainingId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Training.Training), request.TrainingId);

            training.RemoveParticipant(currentUserId);

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

public sealed class LeaveTrainingCommandValidator : AbstractValidator<LeaveTrainingCommand>
{
    public LeaveTrainingCommandValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
    }
}

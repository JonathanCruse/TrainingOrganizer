using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Commands;

public sealed record AcceptTrainingParticipantCommand(Guid TrainingId, Guid MemberId) : IRequest<Result>;

public sealed class AcceptTrainingParticipantCommandHandler : IRequestHandler<AcceptTrainingParticipantCommand, Result>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptTrainingParticipantCommandHandler(
        ITrainingRepository trainingRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AcceptTrainingParticipantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.IsAdmin && !_currentUserService.IsTrainer)
                throw new ForbiddenException("Only admins or trainers can accept participants.");

            var trainingId = new TrainingId(request.TrainingId);
            var training = await _trainingRepository.GetByIdAsync(trainingId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Training.Training), request.TrainingId);

            training.AcceptParticipant(new MemberId(request.MemberId));

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

public sealed class AcceptTrainingParticipantCommandValidator : AbstractValidator<AcceptTrainingParticipantCommand>
{
    public AcceptTrainingParticipantCommandValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

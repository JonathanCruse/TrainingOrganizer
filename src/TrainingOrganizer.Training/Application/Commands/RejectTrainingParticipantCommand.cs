using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Commands;

public sealed record RejectTrainingParticipantCommand(Guid TrainingId, Guid MemberId) : IRequest<Result>;

public sealed class RejectTrainingParticipantCommandHandler : IRequestHandler<RejectTrainingParticipantCommand, Result>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectTrainingParticipantCommandHandler(
        ITrainingRepository trainingRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RejectTrainingParticipantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.IsAdmin && !_currentUserService.IsTrainer)
                throw new ForbiddenException("Only admins or trainers can reject participants.");

            var trainingId = new TrainingId(request.TrainingId);
            var training = await _trainingRepository.GetByIdAsync(trainingId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Training), request.TrainingId);

            training.RejectParticipant(new MemberId(request.MemberId));

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

public sealed class RejectTrainingParticipantCommandValidator : AbstractValidator<RejectTrainingParticipantCommand>
{
    public RejectTrainingParticipantCommandValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

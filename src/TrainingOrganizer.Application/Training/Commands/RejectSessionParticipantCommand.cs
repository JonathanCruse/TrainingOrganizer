using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Commands;

public sealed record RejectSessionParticipantCommand(Guid SessionId, Guid MemberId) : IRequest<Result>;

public sealed class RejectSessionParticipantCommandHandler : IRequestHandler<RejectSessionParticipantCommand, Result>
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectSessionParticipantCommandHandler(
        ITrainingSessionRepository sessionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RejectSessionParticipantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.IsAdmin && !_currentUserService.IsTrainer)
                throw new ForbiddenException("Only admins or trainers can reject participants.");

            var sessionId = new TrainingSessionId(request.SessionId);
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
                ?? throw new NotFoundException(nameof(TrainingSession), request.SessionId);

            session.RejectParticipant(new MemberId(request.MemberId));

            await _sessionRepository.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure("Session.DomainError", ex.Message);
        }
    }
}

public sealed class RejectSessionParticipantCommandValidator : AbstractValidator<RejectSessionParticipantCommand>
{
    public RejectSessionParticipantCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

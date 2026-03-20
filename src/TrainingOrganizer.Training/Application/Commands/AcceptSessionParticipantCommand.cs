using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Commands;

public sealed record AcceptSessionParticipantCommand(Guid SessionId, Guid MemberId) : IRequest<Result>;

public sealed class AcceptSessionParticipantCommandHandler : IRequestHandler<AcceptSessionParticipantCommand, Result>
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptSessionParticipantCommandHandler(
        ITrainingSessionRepository sessionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AcceptSessionParticipantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.IsAdmin && !_currentUserService.IsTrainer)
                throw new ForbiddenException("Only admins or trainers can accept participants.");

            var sessionId = new TrainingSessionId(request.SessionId);
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
                ?? throw new NotFoundException(nameof(TrainingSession), request.SessionId);

            session.AcceptParticipant(new MemberId(request.MemberId));

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

public sealed class AcceptSessionParticipantCommandValidator : AbstractValidator<AcceptSessionParticipantCommand>
{
    public AcceptSessionParticipantCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

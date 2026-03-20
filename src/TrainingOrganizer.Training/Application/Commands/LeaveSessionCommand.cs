using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Commands;

public sealed record LeaveSessionCommand(Guid SessionId) : IRequest<Result>;

public sealed class LeaveSessionCommandHandler : IRequestHandler<LeaveSessionCommand, Result>
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveSessionCommandHandler(
        ITrainingSessionRepository sessionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LeaveSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = new MemberId(_currentUserService.MemberId
                ?? throw new ForbiddenException("You must be authenticated to leave a session."));

            var sessionId = new TrainingSessionId(request.SessionId);
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
                ?? throw new NotFoundException(nameof(TrainingSession), request.SessionId);

            session.RemoveParticipant(currentUserId);

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

public sealed class LeaveSessionCommandValidator : AbstractValidator<LeaveSessionCommand>
{
    public LeaveSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}

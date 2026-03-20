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

public sealed record JoinSessionCommand(Guid SessionId) : IRequest<Result>;

public sealed class JoinSessionCommandHandler : IRequestHandler<JoinSessionCommand, Result>
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public JoinSessionCommandHandler(
        ITrainingSessionRepository sessionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(JoinSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = new MemberId(_currentUserService.MemberId
                ?? throw new ForbiddenException("You must be authenticated to join a session."));

            var sessionId = new TrainingSessionId(request.SessionId);
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
                ?? throw new NotFoundException(nameof(TrainingSession), request.SessionId);

            if (_currentUserService.IsGuest)
                session.RequestGuestParticipation(currentUserId);
            else
                session.AddParticipant(currentUserId);

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

public sealed class JoinSessionCommandValidator : AbstractValidator<JoinSessionCommand>
{
    public JoinSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}

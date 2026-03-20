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

public sealed record CancelSessionCommand(Guid SessionId, string Reason) : IRequest<Result>;

public sealed class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, Result>
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSessionCommandHandler(
        ITrainingSessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sessionId = new TrainingSessionId(request.SessionId);
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
                ?? throw new NotFoundException(nameof(TrainingSession), request.SessionId);

            session.Cancel(request.Reason);

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

public sealed class CancelSessionCommandValidator : AbstractValidator<CancelSessionCommand>
{
    public CancelSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

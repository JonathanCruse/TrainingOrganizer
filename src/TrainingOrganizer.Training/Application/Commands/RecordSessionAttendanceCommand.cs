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

public sealed record RecordSessionAttendanceCommand(
    Guid SessionId,
    List<AttendanceEntry> Entries) : IRequest<Result>;

public sealed class RecordSessionAttendanceCommandHandler : IRequestHandler<RecordSessionAttendanceCommand, Result>
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordSessionAttendanceCommandHandler(
        ITrainingSessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RecordSessionAttendanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sessionId = new TrainingSessionId(request.SessionId);
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
                ?? throw new NotFoundException(nameof(TrainingSession), request.SessionId);

            foreach (var entry in request.Entries)
            {
                var memberId = new MemberId(entry.MemberId);
                session.RecordAttendance(memberId, entry.Attended);
            }

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

public sealed class RecordSessionAttendanceCommandValidator : AbstractValidator<RecordSessionAttendanceCommand>
{
    public RecordSessionAttendanceCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Entries).NotEmpty();
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.MemberId).NotEmpty();
        });
    }
}

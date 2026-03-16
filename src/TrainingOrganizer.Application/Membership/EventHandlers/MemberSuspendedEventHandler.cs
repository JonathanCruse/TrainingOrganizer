using MediatR;
using TrainingOrganizer.Application.Common;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Membership.Events;

namespace TrainingOrganizer.Application.Membership.EventHandlers;

public sealed class MemberSuspendedEventHandler : INotificationHandler<DomainEventNotification<MemberSuspendedEvent>>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MemberSuspendedEventHandler(
        ITrainingRepository trainingRepository,
        ITrainingSessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DomainEventNotification<MemberSuspendedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var trainings = await _trainingRepository.GetByMemberParticipationAsync(domainEvent.MemberId, cancellationToken);
        foreach (var training in trainings)
        {
            training.RemoveParticipant(domainEvent.MemberId);
            await _trainingRepository.UpdateAsync(training, cancellationToken);
        }

        var sessions = await _sessionRepository.GetByMemberParticipationAsync(domainEvent.MemberId, cancellationToken);
        foreach (var session in sessions)
        {
            session.RemoveParticipant(domainEvent.MemberId);
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

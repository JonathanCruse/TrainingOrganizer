using MediatR;
using TrainingOrganizer.Application.Common;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.Events;

namespace TrainingOrganizer.Application.Training.EventHandlers;

public sealed class SessionsRequestedEventHandler : INotificationHandler<DomainEventNotification<SessionsRequestedEvent>>
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SessionsRequestedEventHandler(
        ITrainingSessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DomainEventNotification<SessionsRequestedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var sessions = new List<TrainingSession>();

        foreach (var date in domainEvent.OccurrenceDates)
        {
            var start = new DateTimeOffset(
                date.ToDateTime(domainEvent.RecurrenceRule.TimeOfDay),
                TimeSpan.Zero);
            var end = start.Add(domainEvent.RecurrenceRule.Duration);
            var timeSlot = new TimeSlot(start, end);

            var session = TrainingSession.CreateFromTemplate(
                domainEvent.RecurringTrainingId,
                timeSlot,
                domainEvent.Template);

            sessions.Add(session);
        }

        await _sessionRepository.AddRangeAsync(sessions, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

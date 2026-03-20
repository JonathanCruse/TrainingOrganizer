using MediatR;
using TrainingOrganizer.SharedKernel.Application;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.Events;

namespace TrainingOrganizer.Training.Application.EventHandlers;

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

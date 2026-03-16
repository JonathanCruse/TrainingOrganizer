using MediatR;
using TrainingOrganizer.Application.Common;
using TrainingOrganizer.Domain.Facility.ValueObjects;
using TrainingOrganizer.Domain.Services;
using TrainingOrganizer.Domain.Training.Events;

namespace TrainingOrganizer.Application.Training.EventHandlers;

public sealed class TrainingSessionCanceledEventHandler : INotificationHandler<DomainEventNotification<TrainingSessionCanceledEvent>>
{
    private readonly IRoomBookingService _roomBookingService;

    public TrainingSessionCanceledEventHandler(IRoomBookingService roomBookingService)
    {
        _roomBookingService = roomBookingService;
    }

    public async Task Handle(DomainEventNotification<TrainingSessionCanceledEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var reference = new BookingReference(BookingReferenceType.TrainingSession, domainEvent.TrainingSessionId.Value);

        await _roomBookingService.CancelBookingsForReferenceAsync(reference, cancellationToken);
    }
}

using MediatR;
using TrainingOrganizer.Application.Common;
using TrainingOrganizer.Domain.Facility.ValueObjects;
using TrainingOrganizer.Domain.Services;
using TrainingOrganizer.Domain.Training.Events;

namespace TrainingOrganizer.Application.Training.EventHandlers;

public sealed class TrainingCanceledEventHandler : INotificationHandler<DomainEventNotification<TrainingCanceledEvent>>
{
    private readonly IRoomBookingService _roomBookingService;

    public TrainingCanceledEventHandler(IRoomBookingService roomBookingService)
    {
        _roomBookingService = roomBookingService;
    }

    public async Task Handle(DomainEventNotification<TrainingCanceledEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var reference = new BookingReference(BookingReferenceType.Training, domainEvent.TrainingId.Value);

        await _roomBookingService.CancelBookingsForReferenceAsync(reference, cancellationToken);
    }
}

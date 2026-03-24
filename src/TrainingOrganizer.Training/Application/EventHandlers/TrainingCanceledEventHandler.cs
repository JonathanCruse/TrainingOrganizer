using MediatR;
using TrainingOrganizer.SharedKernel.Application;
using TrainingOrganizer.Facility.Domain.ValueObjects;
using TrainingOrganizer.Facility.Domain.Services;
using TrainingOrganizer.Training.Domain.Events;

namespace TrainingOrganizer.Training.Application.EventHandlers;

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

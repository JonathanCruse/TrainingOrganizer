using MediatR;
using TrainingOrganizer.SharedKernel.Application;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Domain.Facility.ValueObjects;
using TrainingOrganizer.Training.Domain.Services;
using TrainingOrganizer.Training.Domain.Events;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.EventHandlers;

public sealed class TrainingPublishedEventHandler : INotificationHandler<DomainEventNotification<TrainingPublishedEvent>>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly IRoomBookingService _roomBookingService;

    public TrainingPublishedEventHandler(
        ITrainingRepository trainingRepository,
        IRoomBookingService roomBookingService)
    {
        _trainingRepository = trainingRepository;
        _roomBookingService = roomBookingService;
    }

    public async Task Handle(DomainEventNotification<TrainingPublishedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var training = await _trainingRepository.GetByIdAsync(domainEvent.TrainingId, cancellationToken);
        if (training is null) return;

        var reference = new BookingReference(BookingReferenceType.Training, domainEvent.TrainingId.Value);

        foreach (var requirement in domainEvent.RoomRequirements)
        {
            await _roomBookingService.BookRoomAsync(
                requirement.RoomId,
                requirement.LocationId,
                training.TimeSlot,
                reference,
                training.CreatedBy.Value,
                cancellationToken);
        }
    }
}

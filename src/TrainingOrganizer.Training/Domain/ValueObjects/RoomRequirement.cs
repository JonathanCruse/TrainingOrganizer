using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Domain.ValueObjects;

public sealed record RoomRequirement : ValueObject
{
    public RoomId RoomId { get; }
    public LocationId LocationId { get; }

    public RoomRequirement(RoomId roomId, LocationId locationId)
    {
        RoomId = Guard.AgainstNull(roomId, nameof(roomId));
        LocationId = Guard.AgainstNull(locationId, nameof(locationId));
    }
}

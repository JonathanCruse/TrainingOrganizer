using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Domain.Training.ValueObjects;

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

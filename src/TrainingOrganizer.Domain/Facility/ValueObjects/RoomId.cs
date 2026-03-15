using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Facility.ValueObjects;

public sealed record RoomId : StronglyTypedId
{
    public RoomId(Guid value) : base(value) { }

    public static RoomId Create() => new(Guid.NewGuid());
}

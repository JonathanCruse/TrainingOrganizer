using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Facility.Domain.ValueObjects;

public sealed record RoomId : StronglyTypedId
{
    public RoomId(Guid value) : base(value) { }

    public static RoomId Create() => new(Guid.NewGuid());
}

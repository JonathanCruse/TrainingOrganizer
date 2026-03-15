using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Facility.ValueObjects;

public sealed record BookingId : StronglyTypedId
{
    public BookingId(Guid value) : base(value) { }

    public static BookingId Create() => new(Guid.NewGuid());
}

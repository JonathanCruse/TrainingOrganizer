using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Facility.Domain.ValueObjects;

public sealed record BookingId : StronglyTypedId
{
    public BookingId(Guid value) : base(value) { }

    public static BookingId Create() => new(Guid.NewGuid());
}

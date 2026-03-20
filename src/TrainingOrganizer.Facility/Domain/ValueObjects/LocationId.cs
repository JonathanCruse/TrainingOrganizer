using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Facility.Domain.ValueObjects;

public sealed record LocationId : StronglyTypedId
{
    public LocationId(Guid value) : base(value) { }

    public static LocationId Create() => new(Guid.NewGuid());
}

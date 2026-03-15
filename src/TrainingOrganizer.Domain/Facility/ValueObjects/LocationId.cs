using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Facility.ValueObjects;

public sealed record LocationId : StronglyTypedId
{
    public LocationId(Guid value) : base(value) { }

    public static LocationId Create() => new(Guid.NewGuid());
}

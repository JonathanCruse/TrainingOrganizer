using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Training.ValueObjects;

public sealed record TrainingId : StronglyTypedId
{
    public TrainingId(Guid value) : base(value) { }

    public static TrainingId Create() => new(Guid.NewGuid());
}

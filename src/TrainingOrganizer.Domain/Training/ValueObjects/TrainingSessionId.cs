using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Training.ValueObjects;

public sealed record TrainingSessionId : StronglyTypedId
{
    public TrainingSessionId(Guid value) : base(value) { }

    public static TrainingSessionId Create() => new(Guid.NewGuid());
}

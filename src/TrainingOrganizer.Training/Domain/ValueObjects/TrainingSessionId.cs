using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Training.Domain.ValueObjects;

public sealed record TrainingSessionId : StronglyTypedId
{
    public TrainingSessionId(Guid value) : base(value) { }

    public static TrainingSessionId Create() => new(Guid.NewGuid());
}

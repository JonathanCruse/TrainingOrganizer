using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Training.Domain.ValueObjects;

public sealed record TrainingId : StronglyTypedId
{
    public TrainingId(Guid value) : base(value) { }

    public static TrainingId Create() => new(Guid.NewGuid());
}

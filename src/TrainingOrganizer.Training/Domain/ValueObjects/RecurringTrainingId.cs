using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Training.Domain.ValueObjects;

public sealed record RecurringTrainingId : StronglyTypedId
{
    public RecurringTrainingId(Guid value) : base(value) { }

    public static RecurringTrainingId Create() => new(Guid.NewGuid());
}

using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Training.ValueObjects;

public sealed record RecurringTrainingId : StronglyTypedId
{
    public RecurringTrainingId(Guid value) : base(value) { }

    public static RecurringTrainingId Create() => new(Guid.NewGuid());
}

namespace TrainingOrganizer.SharedKernel.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

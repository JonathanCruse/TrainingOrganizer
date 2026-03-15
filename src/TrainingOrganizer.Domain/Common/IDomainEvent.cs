namespace TrainingOrganizer.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

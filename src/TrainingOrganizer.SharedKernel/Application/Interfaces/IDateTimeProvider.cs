namespace TrainingOrganizer.SharedKernel.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
    DateOnly Today { get; }
}

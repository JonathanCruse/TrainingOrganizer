namespace TrainingOrganizer.Api.Contracts;

public sealed record CreateRecurringTrainingRequest(
    string Title,
    string? Description,
    int MinCapacity,
    int MaxCapacity,
    string Visibility,
    List<Guid> TrainerIds,
    string Pattern,
    DayOfWeek DayOfWeek,
    string TimeOfDay,
    string Duration,
    string StartDate,
    string? EndDate);

public sealed record GenerateSessionsRequest(
    DateTimeOffset Until);

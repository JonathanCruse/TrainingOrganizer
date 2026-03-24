namespace TrainingOrganizer.Api.Contracts;

public sealed record CreateRecurringTrainingRequest(
    string Title,
    string? Description,
    int MinCapacity,
    int MaxCapacity,
    string Visibility,
    List<Guid> TrainerIds,
    List<RoomRequirementRequest> RoomRequirements,
    string Pattern,
    DayOfWeek DayOfWeek,
    string TimeOfDay,
    string Duration,
    string StartDate,
    string? EndDate);

public sealed record RoomRequirementRequest(Guid RoomId, Guid LocationId);

public sealed record GenerateSessionsRequest(
    DateTimeOffset Until);

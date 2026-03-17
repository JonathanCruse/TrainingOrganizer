using TrainingOrganizer.Shared.Enums;

namespace TrainingOrganizer.Shared.Models;

public sealed record RecurringTrainingResponse(
    Guid Id,
    string Title,
    string Description,
    int MinCapacity,
    int MaxCapacity,
    Visibility Visibility,
    IReadOnlyList<Guid> TrainerIds,
    IReadOnlyList<RoomRequirementResponse> RoomRequirements,
    RecurrencePattern Pattern,
    DayOfWeek DayOfWeek,
    string TimeOfDay,
    string Duration,
    string StartDate,
    string? EndDate,
    RecurringTrainingStatus Status,
    string? LastGeneratedUntil,
    DateTimeOffset CreatedAt,
    Guid CreatedBy);

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

public sealed record GenerateSessionsRequest(DateTimeOffset Until);

using TrainingOrganizer.Shared.Enums;

namespace TrainingOrganizer.Shared.Models;

public sealed record TrainingSessionResponse(
    Guid Id,
    Guid RecurringTrainingId,
    string Title,
    string Description,
    DateTimeOffset Start,
    DateTimeOffset End,
    int MinCapacity,
    int MaxCapacity,
    Visibility Visibility,
    SessionStatus Status,
    IReadOnlyList<Guid> TrainerIds,
    IReadOnlyList<ParticipantResponse> Participants,
    IReadOnlyList<RoomRequirementResponse> RoomRequirements,
    bool HasOverrides,
    int ConfirmedParticipantCount,
    int WaitlistCount,
    DateTimeOffset CreatedAt);

public sealed record ApplySessionOverridesRequest(
    string? Title,
    string? Description,
    int? MinCapacity,
    int? MaxCapacity,
    string? Visibility);

public sealed record RecordSessionAttendanceRequest(List<AttendanceEntryRequest> Entries);
public sealed record CancelSessionRequest(string Reason);

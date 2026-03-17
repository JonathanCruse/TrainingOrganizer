using TrainingOrganizer.Shared.Enums;

namespace TrainingOrganizer.Shared.Models;

public sealed record TrainingResponse(
    Guid Id,
    string Title,
    string Description,
    DateTimeOffset Start,
    DateTimeOffset End,
    int MinCapacity,
    int MaxCapacity,
    Visibility Visibility,
    TrainingStatus Status,
    IReadOnlyList<Guid> TrainerIds,
    IReadOnlyList<ParticipantResponse> Participants,
    IReadOnlyList<RoomRequirementResponse> RoomRequirements,
    int ConfirmedParticipantCount,
    int WaitlistCount,
    DateTimeOffset CreatedAt,
    Guid CreatedBy);

public sealed record ParticipantResponse(
    Guid MemberId,
    ParticipationStatus Status,
    DateTimeOffset JoinedAt,
    int? WaitlistPosition,
    bool AttendanceRecorded,
    bool Attended);

public sealed record RoomRequirementResponse(Guid RoomId, Guid LocationId);

public sealed record CreateTrainingRequest(
    string Title,
    string? Description,
    DateTimeOffset Start,
    DateTimeOffset End,
    int MinCapacity,
    int MaxCapacity,
    string Visibility,
    List<Guid> TrainerIds);

public sealed record UpdateTrainingRequest(
    string Title,
    string? Description,
    DateTimeOffset Start,
    DateTimeOffset End,
    int MinCapacity,
    int MaxCapacity,
    string Visibility);

public sealed record CancelTrainingRequest(string Reason);
public sealed record RecordAttendanceRequest(List<AttendanceEntryRequest> Entries);
public sealed record AttendanceEntryRequest(Guid MemberId, bool Attended);
public sealed record AssignTrainerRequest(Guid TrainerId);

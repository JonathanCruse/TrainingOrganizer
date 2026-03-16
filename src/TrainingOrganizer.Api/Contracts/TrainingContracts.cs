namespace TrainingOrganizer.Api.Contracts;

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

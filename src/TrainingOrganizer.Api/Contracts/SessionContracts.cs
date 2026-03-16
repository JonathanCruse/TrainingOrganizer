namespace TrainingOrganizer.Api.Contracts;

public sealed record ApplySessionOverridesRequest(
    string? Title,
    string? Description,
    int? MinCapacity,
    int? MaxCapacity,
    string? Visibility,
    List<Guid>? TrainerIds);

public sealed record RecordSessionAttendanceRequest(List<AttendanceEntryRequest> Entries);

public sealed record CancelSessionRequest(string Reason);

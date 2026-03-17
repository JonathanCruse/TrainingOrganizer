namespace TrainingOrganizer.Shared.Models;

public sealed record ScheduleEntryResponse(
    Guid Id,
    string Type,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? LocationName,
    string? RoomName);

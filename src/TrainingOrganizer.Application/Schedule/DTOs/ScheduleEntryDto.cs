namespace TrainingOrganizer.Application.Schedule.DTOs;

public sealed record ScheduleEntryDto(
    Guid Id,
    string Type,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? LocationName,
    string? RoomName);

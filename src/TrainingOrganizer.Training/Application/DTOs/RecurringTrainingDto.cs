using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.Enums;

namespace TrainingOrganizer.Training.Application.DTOs;

public sealed record RecurringTrainingDto(
    Guid Id,
    string Title,
    string Description,
    int MinCapacity,
    int MaxCapacity,
    Visibility Visibility,
    IReadOnlyList<Guid> TrainerIds,
    IReadOnlyList<RoomRequirementDto> RoomRequirements,
    RecurrencePattern Pattern,
    DayOfWeek DayOfWeek,
    string TimeOfDay,
    string Duration,
    string StartDate,
    string? EndDate,
    RecurringTrainingStatus Status,
    string? LastGeneratedUntil,
    DateTimeOffset CreatedAt,
    Guid CreatedBy)
{
    public static RecurringTrainingDto FromDomain(RecurringTraining rt) => new(
        rt.Id.Value,
        rt.Template.Title.Value,
        rt.Template.Description.Value,
        rt.Template.Capacity.Min,
        rt.Template.Capacity.Max,
        rt.Template.Visibility,
        rt.Template.TrainerIds.Select(t => t.Value).ToList(),
        rt.Template.RoomRequirements.Select(RoomRequirementDto.FromDomain).ToList(),
        rt.RecurrenceRule.Pattern,
        rt.RecurrenceRule.DayOfWeek,
        rt.RecurrenceRule.TimeOfDay.ToString("HH:mm"),
        rt.RecurrenceRule.Duration.ToString(),
        rt.RecurrenceRule.StartDate.ToString("yyyy-MM-dd"),
        rt.RecurrenceRule.EndDate?.ToString("yyyy-MM-dd"),
        rt.Status,
        rt.LastGeneratedUntil?.ToString("yyyy-MM-dd"),
        rt.CreatedAt,
        rt.CreatedBy.Value);
}

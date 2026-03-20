using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using MongoDB.Bson.Serialization.Attributes;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Infrastructure.Persistence.Documents;

public sealed class RecurringTrainingDocument
{
    [BsonId]
    public Guid Id { get; set; }

    // Template fields
    public string TemplateTitle { get; set; } = string.Empty;
    public string TemplateDescription { get; set; } = string.Empty;
    public int TemplateCapacityMin { get; set; }
    public int TemplateCapacityMax { get; set; }
    public string TemplateVisibility { get; set; } = string.Empty;
    public List<Guid> TemplateTrainerIds { get; set; } = [];
    public List<RoomRequirementDocument> TemplateRoomRequirements { get; set; } = [];

    // RecurrenceRule fields
    public string RecurrencePattern { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public string TimeOfDay { get; set; } = string.Empty;
    public long DurationTicks { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string? EndDate { get; set; }

    public string Status { get; set; } = string.Empty;
    public string? LastGeneratedUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public int Version { get; set; }

    public static RecurringTrainingDocument FromDomain(RecurringTraining recurring)
    {
        return new RecurringTrainingDocument
        {
            Id = recurring.Id.Value,
            TemplateTitle = recurring.Template.Title.Value,
            TemplateDescription = recurring.Template.Description.Value,
            TemplateCapacityMin = recurring.Template.Capacity.Min,
            TemplateCapacityMax = recurring.Template.Capacity.Max,
            TemplateVisibility = recurring.Template.Visibility.ToString(),
            TemplateTrainerIds = recurring.Template.TrainerIds.Select(t => t.Value).ToList(),
            TemplateRoomRequirements = recurring.Template.RoomRequirements
                .Select(RoomRequirementDocument.FromDomain).ToList(),
            RecurrencePattern = recurring.RecurrenceRule.Pattern.ToString(),
            DayOfWeek = recurring.RecurrenceRule.DayOfWeek.ToString(),
            TimeOfDay = recurring.RecurrenceRule.TimeOfDay.ToString("HH:mm:ss"),
            DurationTicks = recurring.RecurrenceRule.Duration.Ticks,
            StartDate = recurring.RecurrenceRule.StartDate.ToString("O"),
            EndDate = recurring.RecurrenceRule.EndDate?.ToString("O"),
            Status = recurring.Status.ToString(),
            LastGeneratedUntil = recurring.LastGeneratedUntil?.ToString("O"),
            CreatedAt = recurring.CreatedAt,
            CreatedBy = recurring.CreatedBy.Value,
            Version = recurring.Version
        };
    }

    public RecurringTraining ToDomain()
    {
        var recurring = DomainObjectMapper.CreateInstance<RecurringTraining>();

        DomainObjectMapper.SetProperty(recurring, "Id", new RecurringTrainingId(Id));

        var template = new TrainingTemplate(
            new TrainingTitle(TemplateTitle),
            new TrainingDescription(TemplateDescription),
            new Capacity(TemplateCapacityMin, TemplateCapacityMax),
            Enum.Parse<Visibility>(TemplateVisibility),
            TemplateTrainerIds.Select(t => new MemberId(t)).ToList(),
            TemplateRoomRequirements.Select(r => r.ToDomain()).ToList());
        DomainObjectMapper.SetProperty(recurring, "Template", template);

        var recurrenceRule = new RecurrenceRule(
            Enum.Parse<RecurrencePattern>(this.RecurrencePattern),
            Enum.Parse<System.DayOfWeek>(DayOfWeek),
            TimeOnly.Parse(TimeOfDay),
            TimeSpan.FromTicks(DurationTicks),
            DateOnly.Parse(StartDate),
            EndDate is not null ? DateOnly.Parse(EndDate) : null);
        DomainObjectMapper.SetProperty(recurring, "RecurrenceRule", recurrenceRule);

        DomainObjectMapper.SetProperty(recurring, "Status",
            Enum.Parse<RecurringTrainingStatus>(Status));
        DomainObjectMapper.SetProperty(recurring, "LastGeneratedUntil",
            LastGeneratedUntil is not null ? DateOnly.Parse(LastGeneratedUntil) : (DateOnly?)null);
        DomainObjectMapper.SetProperty(recurring, "CreatedAt", CreatedAt);
        DomainObjectMapper.SetProperty(recurring, "CreatedBy", new MemberId(CreatedBy));
        DomainObjectMapper.SetProperty(recurring, "Version", Version);

        return recurring;
    }
}

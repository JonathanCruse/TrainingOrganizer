using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Infrastructure.Persistence.Documents;

public sealed class SessionOverridesDocument
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? CapacityMin { get; set; }
    public int? CapacityMax { get; set; }
    public string? Visibility { get; set; }
    public List<Guid>? TrainerIds { get; set; }
    public List<RoomRequirementDocument>? RoomRequirements { get; set; }

    public static SessionOverridesDocument FromDomain(SessionOverrides overrides)
    {
        return new SessionOverridesDocument
        {
            Title = overrides.Title?.Value,
            Description = overrides.Description?.Value,
            CapacityMin = overrides.Capacity?.Min,
            CapacityMax = overrides.Capacity?.Max,
            Visibility = overrides.Visibility?.ToString(),
            TrainerIds = overrides.TrainerIds?.Select(t => t.Value).ToList(),
            RoomRequirements = overrides.RoomRequirements
                ?.Select(RoomRequirementDocument.FromDomain).ToList()
        };
    }

    public SessionOverrides ToDomain()
    {
        return new SessionOverrides
        {
            Title = Title is not null ? new TrainingTitle(Title) : null,
            Description = Description is not null ? new TrainingDescription(Description) : null,
            Capacity = CapacityMin.HasValue && CapacityMax.HasValue
                ? new Capacity(CapacityMin.Value, CapacityMax.Value)
                : null,
            Visibility = Visibility is not null
                ? Enum.Parse<Visibility>(Visibility)
                : null,
            TrainerIds = TrainerIds?.Select(t => new MemberId(t)).ToList(),
            RoomRequirements = RoomRequirements?.Select(r => r.ToDomain()).ToList()
        };
    }
}

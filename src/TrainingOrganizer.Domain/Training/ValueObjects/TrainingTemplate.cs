using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;

namespace TrainingOrganizer.Domain.Training.ValueObjects;

public sealed record TrainingTemplate : ValueObject
{
    public TrainingTitle Title { get; }
    public TrainingDescription Description { get; }
    public Capacity Capacity { get; }
    public Visibility Visibility { get; }
    public IReadOnlyList<MemberId> TrainerIds { get; }
    public IReadOnlyList<RoomRequirement> RoomRequirements { get; }

    public TrainingTemplate(
        TrainingTitle title,
        TrainingDescription description,
        Capacity capacity,
        Visibility visibility,
        IReadOnlyList<MemberId> trainerIds,
        IReadOnlyList<RoomRequirement> roomRequirements)
    {
        Guard.AgainstNull(title, nameof(title));
        Guard.AgainstNull(description, nameof(description));
        Guard.AgainstNull(capacity, nameof(capacity));
        Guard.AgainstNull(trainerIds, nameof(trainerIds));
        Guard.AgainstCondition(trainerIds.Count == 0, "A training template must have at least one trainer.");
        Guard.AgainstNull(roomRequirements, nameof(roomRequirements));

        Title = title;
        Description = description;
        Capacity = capacity;
        Visibility = visibility;
        TrainerIds = trainerIds;
        RoomRequirements = roomRequirements;
    }
}

using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;

namespace TrainingOrganizer.Domain.Training.ValueObjects;

public sealed record SessionOverrides : ValueObject
{
    public TrainingTitle? Title { get; init; }
    public TrainingDescription? Description { get; init; }
    public Capacity? Capacity { get; init; }
    public Visibility? Visibility { get; init; }
    public IReadOnlyList<MemberId>? TrainerIds { get; init; }
    public IReadOnlyList<RoomRequirement>? RoomRequirements { get; init; }

    public bool HasAnyOverride =>
        Title is not null ||
        Description is not null ||
        Capacity is not null ||
        Visibility is not null ||
        TrainerIds is not null ||
        RoomRequirements is not null;

    public static SessionOverrides None => new();
}

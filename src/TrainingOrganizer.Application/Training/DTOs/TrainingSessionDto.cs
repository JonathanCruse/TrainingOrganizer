using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.Enums;

namespace TrainingOrganizer.Application.Training.DTOs;

public sealed record TrainingSessionDto(
    Guid Id,
    Guid RecurringTrainingId,
    string Title,
    string Description,
    DateTimeOffset Start,
    DateTimeOffset End,
    int MinCapacity,
    int MaxCapacity,
    Visibility Visibility,
    SessionStatus Status,
    IReadOnlyList<Guid> TrainerIds,
    IReadOnlyList<ParticipantDto> Participants,
    IReadOnlyList<RoomRequirementDto> RoomRequirements,
    bool HasOverrides,
    int ConfirmedParticipantCount,
    int WaitlistCount,
    DateTimeOffset CreatedAt)
{
    public static TrainingSessionDto FromDomain(TrainingSession session) => new(
        session.Id.Value,
        session.RecurringTrainingId.Value,
        session.EffectiveTitle.Value,
        session.EffectiveDescription.Value,
        session.TimeSlot.Start,
        session.TimeSlot.End,
        session.EffectiveCapacity.Min,
        session.EffectiveCapacity.Max,
        session.EffectiveVisibility,
        session.Status,
        session.EffectiveTrainerIds.Select(t => t.Value).ToList(),
        session.Participants.Select(ParticipantDto.FromDomain).ToList(),
        session.EffectiveRoomRequirements.Select(RoomRequirementDto.FromDomain).ToList(),
        session.Overrides.HasAnyOverride,
        session.ConfirmedParticipantCount,
        session.WaitlistCount,
        session.CreatedAt);
}

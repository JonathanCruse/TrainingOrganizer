using TrainingOrganizer.Training.Domain.Enums;

namespace TrainingOrganizer.Training.Application.DTOs;

public sealed record TrainingDto(
    Guid Id,
    string Title,
    string Description,
    DateTimeOffset Start,
    DateTimeOffset End,
    int MinCapacity,
    int MaxCapacity,
    Visibility Visibility,
    TrainingStatus Status,
    IReadOnlyList<Guid> TrainerIds,
    IReadOnlyList<ParticipantDto> Participants,
    IReadOnlyList<RoomRequirementDto> RoomRequirements,
    int ConfirmedParticipantCount,
    int WaitlistCount,
    DateTimeOffset CreatedAt,
    Guid CreatedBy)
{
    public static TrainingDto FromDomain(Domain.Training training) => new(
        training.Id.Value,
        training.Title.Value,
        training.Description.Value,
        training.TimeSlot.Start,
        training.TimeSlot.End,
        training.Capacity.Min,
        training.Capacity.Max,
        training.Visibility,
        training.Status,
        training.TrainerIds.Select(t => t.Value).ToList(),
        training.Participants.Select(ParticipantDto.FromDomain).ToList(),
        training.RoomRequirements.Select(RoomRequirementDto.FromDomain).ToList(),
        training.ConfirmedParticipantCount,
        training.WaitlistCount,
        training.CreatedAt,
        training.CreatedBy.Value);
}

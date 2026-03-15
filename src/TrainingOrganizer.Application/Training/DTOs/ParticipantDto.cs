using TrainingOrganizer.Domain.Training.Entities;
using TrainingOrganizer.Domain.Training.Enums;

namespace TrainingOrganizer.Application.Training.DTOs;

public sealed record ParticipantDto(
    Guid MemberId,
    ParticipationStatus Status,
    DateTimeOffset JoinedAt,
    int? WaitlistPosition,
    bool AttendanceRecorded,
    bool Attended)
{
    public static ParticipantDto FromDomain(Participant participant) => new(
        participant.Id.Value,
        participant.Status,
        participant.JoinedAt,
        participant.WaitlistPosition,
        participant.AttendanceRecorded,
        participant.Attended);
}

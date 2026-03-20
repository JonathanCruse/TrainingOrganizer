using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain.Entities;
using TrainingOrganizer.Training.Domain.Enums;

namespace TrainingOrganizer.Training.Infrastructure.Persistence.Documents;

public sealed class ParticipantDocument
{
    public Guid MemberId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset JoinedAt { get; set; }
    public int? WaitlistPosition { get; set; }
    public bool AttendanceRecorded { get; set; }
    public bool Attended { get; set; }

    public static ParticipantDocument FromDomain(Participant participant)
    {
        return new ParticipantDocument
        {
            MemberId = participant.Id.Value,
            Status = participant.Status.ToString(),
            JoinedAt = participant.JoinedAt,
            WaitlistPosition = participant.WaitlistPosition,
            AttendanceRecorded = participant.AttendanceRecorded,
            Attended = participant.Attended
        };
    }

    public Participant ToDomain()
    {
        var participant = DomainObjectMapper.CreateInstance<Participant>();

        DomainObjectMapper.SetProperty(participant, "Id", new MemberId(MemberId));
        DomainObjectMapper.SetProperty(participant, "Status", Enum.Parse<ParticipationStatus>(Status));
        DomainObjectMapper.SetProperty(participant, "JoinedAt", JoinedAt);
        DomainObjectMapper.SetProperty(participant, "WaitlistPosition", WaitlistPosition);
        DomainObjectMapper.SetProperty(participant, "AttendanceRecorded", AttendanceRecorded);
        DomainObjectMapper.SetProperty(participant, "Attended", Attended);

        return participant;
    }
}

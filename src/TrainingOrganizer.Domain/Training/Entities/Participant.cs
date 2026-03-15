using System.Diagnostics.CodeAnalysis;
using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;

namespace TrainingOrganizer.Domain.Training.Entities;

public sealed class Participant : Entity<MemberId>
{
    public ParticipationStatus Status { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public int? WaitlistPosition { get; private set; }
    public bool AttendanceRecorded { get; private set; }
    public bool Attended { get; private set; }

    [SetsRequiredMembers]
    private Participant() { }

    internal static Participant CreateConfirmed(MemberId memberId)
    {
        return new Participant
        {
            Id = memberId,
            Status = ParticipationStatus.Confirmed,
            JoinedAt = DateTimeOffset.UtcNow,
            WaitlistPosition = null
        };
    }

    internal static Participant CreateWaitlisted(MemberId memberId, int position)
    {
        return new Participant
        {
            Id = memberId,
            Status = ParticipationStatus.Waitlisted,
            JoinedAt = DateTimeOffset.UtcNow,
            WaitlistPosition = position
        };
    }

    internal void Confirm()
    {
        Status = ParticipationStatus.Confirmed;
        WaitlistPosition = null;
    }

    internal void Cancel()
    {
        Status = ParticipationStatus.Canceled;
        WaitlistPosition = null;
    }

    internal void UpdateWaitlistPosition(int position)
    {
        WaitlistPosition = position;
    }

    internal void RecordAttendance(bool attended)
    {
        Guard.AgainstCondition(Status != ParticipationStatus.Confirmed,
            "Can only record attendance for confirmed participants.");

        AttendanceRecorded = true;
        Attended = attended;
    }

    public bool IsConfirmed => Status == ParticipationStatus.Confirmed;
    public bool IsWaitlisted => Status == ParticipationStatus.Waitlisted;
    public bool IsActive => Status is ParticipationStatus.Confirmed or ParticipationStatus.Waitlisted;
}

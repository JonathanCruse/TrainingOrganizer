using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Entities;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Training;

/// <summary>
/// Internal helper that encapsulates participant list management logic
/// shared between Training and TrainingSession aggregates.
/// </summary>
internal sealed class ParticipantManager
{
    private readonly List<Participant> _participants;

    public ParticipantManager(List<Participant> participants)
    {
        _participants = participants;
    }

    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();
    public int ConfirmedCount => _participants.Count(p => p.IsConfirmed);
    public int WaitlistCount => _participants.Count(p => p.IsWaitlisted);

    public (Participant participant, bool wasWaitlisted) AddParticipant(MemberId memberId, Capacity capacity)
    {
        if (_participants.Any(p => p.Id == memberId && (p.IsActive || p.IsPendingApproval)))
            throw new BusinessRuleViolationException(
                "DuplicateParticipant",
                "Member is already participating in this training.");

        if (capacity.IsFull(ConfirmedCount))
        {
            var position = WaitlistCount + 1;
            var waitlisted = Participant.CreateWaitlisted(memberId, position);
            _participants.Add(waitlisted);
            return (waitlisted, true);
        }

        var confirmed = Participant.CreateConfirmed(memberId);
        _participants.Add(confirmed);
        return (confirmed, false);
    }

    public Participant AddGuestParticipant(MemberId memberId)
    {
        if (_participants.Any(p => p.Id == memberId && (p.IsActive || p.IsPendingApproval)))
            throw new BusinessRuleViolationException(
                "DuplicateParticipant",
                "Member is already participating in this training.");

        var pending = Participant.CreatePendingApproval(memberId);
        _participants.Add(pending);
        return pending;
    }

    public (Participant participant, bool wasWaitlisted) AcceptParticipant(MemberId memberId, Capacity capacity)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == memberId && p.IsPendingApproval)
                          ?? throw new EntityNotFoundException("Pending participant", memberId);

        if (capacity.IsFull(ConfirmedCount))
        {
            var position = WaitlistCount + 1;
            participant.UpdateWaitlistPosition(position);
            participant.Waitlist();
            return (participant, true);
        }

        participant.Confirm();
        return (participant, false);
    }

    public Participant RejectParticipant(MemberId memberId)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == memberId && p.IsPendingApproval)
                          ?? throw new EntityNotFoundException("Pending participant", memberId);

        participant.Cancel();
        return participant;
    }

    public int PendingApprovalCount => _participants.Count(p => p.IsPendingApproval);

    public (Participant canceled, Participant? promoted) RemoveParticipant(MemberId memberId)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == memberId && p.IsActive)
                          ?? throw new EntityNotFoundException("Participant", memberId);

        participant.Cancel();

        Participant? promoted = null;
        if (participant.IsConfirmed == false && participant.Status != Enums.ParticipationStatus.Confirmed)
        {
            // Was waitlisted, just reorder waitlist
            ReorderWaitlist();
        }

        // If a confirmed spot opened up, promote from waitlist
        if (participant.Status == Enums.ParticipationStatus.Canceled)
        {
            var wasConfirmed = participant.WaitlistPosition is null; // was confirmed before cancel
            // Check the state before cancel - if participant had no waitlist position, they were confirmed
            promoted = TryPromoteFromWaitlist();
        }

        return (participant, promoted);
    }

    public void RecordAttendance(MemberId memberId, bool attended)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == memberId && p.IsConfirmed)
                          ?? throw new EntityNotFoundException("Confirmed participant", memberId);

        participant.RecordAttendance(attended);
    }

    private Participant? TryPromoteFromWaitlist()
    {
        var next = _participants
            .Where(p => p.IsWaitlisted)
            .OrderBy(p => p.WaitlistPosition)
            .FirstOrDefault();

        if (next is null) return null;

        next.Confirm();
        ReorderWaitlist();
        return next;
    }

    private void ReorderWaitlist()
    {
        var waitlisted = _participants
            .Where(p => p.IsWaitlisted)
            .OrderBy(p => p.JoinedAt)
            .ToList();

        for (var i = 0; i < waitlisted.Count; i++)
        {
            waitlisted[i].UpdateWaitlistPosition(i + 1);
        }
    }
}

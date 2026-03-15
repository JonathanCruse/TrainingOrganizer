using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Membership.Events;

public sealed record MemberRejectedEvent(
    MemberId MemberId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;

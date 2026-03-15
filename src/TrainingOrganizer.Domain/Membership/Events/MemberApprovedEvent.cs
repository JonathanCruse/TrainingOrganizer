using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Membership.Events;

public sealed record MemberApprovedEvent(
    MemberId MemberId,
    MemberId ApprovedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;

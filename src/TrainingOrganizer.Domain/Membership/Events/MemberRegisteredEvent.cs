using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Membership.Events;

public sealed record MemberRegisteredEvent(
    MemberId MemberId,
    Email Email,
    DateTimeOffset OccurredAt) : IDomainEvent;

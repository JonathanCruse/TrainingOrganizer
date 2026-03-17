using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Membership.Events;

public sealed record MemberImportedEvent(
    MemberId MemberId,
    Email Email,
    string Source,
    DateTimeOffset OccurredAt) : IDomainEvent;

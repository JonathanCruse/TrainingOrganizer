using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.Enums;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Membership.Events;

public sealed record RoleRemovedEvent(
    MemberId MemberId,
    MemberRole RemovedRole,
    DateTimeOffset OccurredAt) : IDomainEvent;

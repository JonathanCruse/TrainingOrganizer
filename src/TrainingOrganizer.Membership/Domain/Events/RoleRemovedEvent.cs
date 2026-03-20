using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.Enums;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Domain.Events;

public sealed record RoleRemovedEvent(
    MemberId MemberId,
    MemberRole RemovedRole,
    DateTimeOffset OccurredAt) : IDomainEvent;

using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.Enums;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Domain.Events;

public sealed record RoleAssignedEvent(
    MemberId MemberId,
    MemberRole NewRole,
    DateTimeOffset OccurredAt) : IDomainEvent;

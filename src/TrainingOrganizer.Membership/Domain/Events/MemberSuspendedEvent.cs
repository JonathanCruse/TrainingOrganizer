using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Domain.Events;

public sealed record MemberSuspendedEvent(
    MemberId MemberId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;

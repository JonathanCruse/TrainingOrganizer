using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Domain.Events;

public sealed record MemberRegisteredEvent(
    MemberId MemberId,
    Email Email,
    DateTimeOffset OccurredAt) : IDomainEvent;

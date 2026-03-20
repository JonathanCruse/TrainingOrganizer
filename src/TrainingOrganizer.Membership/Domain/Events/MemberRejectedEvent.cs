using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Domain.Events;

public sealed record MemberRejectedEvent(
    MemberId MemberId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;

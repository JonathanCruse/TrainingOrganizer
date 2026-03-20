using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Domain.Events;

public sealed record MemberApprovedEvent(
    MemberId MemberId,
    MemberId ApprovedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;

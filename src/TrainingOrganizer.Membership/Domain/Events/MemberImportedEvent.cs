using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Domain.Events;

public sealed record MemberImportedEvent(
    MemberId MemberId,
    Email Email,
    string Source,
    DateTimeOffset OccurredAt) : IDomainEvent;

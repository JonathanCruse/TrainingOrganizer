using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Domain.Events;

public sealed record AttendanceRecordedEvent(
    Guid TrainingOrSessionId,
    MemberId MemberId,
    bool Attended,
    DateTimeOffset OccurredAt) : IDomainEvent;

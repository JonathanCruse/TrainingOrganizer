using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record AttendanceRecordedEvent(
    Guid TrainingOrSessionId,
    MemberId MemberId,
    bool Attended,
    DateTimeOffset OccurredAt) : IDomainEvent;

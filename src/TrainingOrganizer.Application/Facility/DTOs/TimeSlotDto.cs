using TrainingOrganizer.Domain.Common.ValueObjects;

namespace TrainingOrganizer.Application.Facility.DTOs;

public sealed record TimeSlotDto(DateTimeOffset Start, DateTimeOffset End)
{
    public static TimeSlotDto FromDomain(TimeSlot timeSlot) => new(timeSlot.Start, timeSlot.End);
}

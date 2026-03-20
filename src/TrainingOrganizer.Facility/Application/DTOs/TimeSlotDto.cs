using TrainingOrganizer.SharedKernel.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Application.DTOs;

public sealed record TimeSlotDto(DateTimeOffset Start, DateTimeOffset End)
{
    public static TimeSlotDto FromDomain(TimeSlot timeSlot) => new(timeSlot.Start, timeSlot.End);
}

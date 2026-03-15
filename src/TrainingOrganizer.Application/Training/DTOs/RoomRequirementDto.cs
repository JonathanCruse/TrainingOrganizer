using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.DTOs;

public sealed record RoomRequirementDto(Guid RoomId, Guid LocationId)
{
    public static RoomRequirementDto FromDomain(RoomRequirement requirement) => new(
        requirement.RoomId.Value,
        requirement.LocationId.Value);
}

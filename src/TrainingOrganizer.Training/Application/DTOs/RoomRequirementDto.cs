using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.DTOs;

public sealed record RoomRequirementDto(Guid RoomId, Guid LocationId)
{
    public static RoomRequirementDto FromDomain(RoomRequirement requirement) => new(
        requirement.RoomId.Value,
        requirement.LocationId.Value);
}

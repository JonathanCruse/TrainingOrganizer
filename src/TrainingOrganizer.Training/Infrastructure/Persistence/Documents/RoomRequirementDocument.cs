using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using TrainingOrganizer.Domain.Facility.ValueObjects;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Infrastructure.Persistence.Documents;

public sealed class RoomRequirementDocument
{
    public Guid RoomId { get; set; }
    public Guid LocationId { get; set; }

    public static RoomRequirementDocument FromDomain(RoomRequirement requirement)
    {
        return new RoomRequirementDocument
        {
            RoomId = requirement.RoomId.Value,
            LocationId = requirement.LocationId.Value
        };
    }

    public RoomRequirement ToDomain()
    {
        return new RoomRequirement(new RoomId(RoomId), new LocationId(LocationId));
    }
}

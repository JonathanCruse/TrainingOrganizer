using TrainingOrganizer.Domain.Facility.Entities;
using TrainingOrganizer.Domain.Facility.Enums;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Infrastructure.Persistence.Documents;

public sealed class RoomDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;

    public static RoomDocument FromDomain(Room room)
    {
        return new RoomDocument
        {
            Id = room.Id.Value,
            Name = room.Name.Value,
            Capacity = room.Capacity,
            Status = room.Status.ToString()
        };
    }

    public Room ToDomain()
    {
        var room = DomainObjectMapper.CreateInstance<Room>();

        DomainObjectMapper.SetProperty(room, "Id", new RoomId(Id));
        DomainObjectMapper.SetProperty(room, "Name", new RoomName(Name));
        DomainObjectMapper.SetProperty(room, "Capacity", Capacity);
        DomainObjectMapper.SetProperty(room, "Status", Enum.Parse<RoomStatus>(Status));

        return room;
    }
}

using TrainingOrganizer.Domain.Facility.Entities;
using TrainingOrganizer.Domain.Facility.Enums;

namespace TrainingOrganizer.Application.Facility.DTOs;

public sealed record RoomDto(
    Guid Id,
    string Name,
    int Capacity,
    RoomStatus Status)
{
    public static RoomDto FromDomain(Room room) => new(
        room.Id.Value,
        room.Name.Value,
        room.Capacity,
        room.Status);
}

using TrainingOrganizer.Facility.Domain.Entities;
using TrainingOrganizer.Facility.Domain.Enums;

namespace TrainingOrganizer.Facility.Application.DTOs;

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

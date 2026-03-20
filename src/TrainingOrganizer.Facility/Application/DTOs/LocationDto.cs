using TrainingOrganizer.Facility.Domain;

namespace TrainingOrganizer.Facility.Application.DTOs;

public sealed record LocationDto(
    Guid Id,
    string Name,
    string Street,
    string City,
    string PostalCode,
    string Country,
    IReadOnlyList<RoomDto> Rooms)
{
    public static LocationDto FromDomain(Location location) => new(
        location.Id.Value,
        location.Name.Value,
        location.Address.Street,
        location.Address.City,
        location.Address.PostalCode,
        location.Address.Country,
        location.Rooms.Select(RoomDto.FromDomain).ToList());
}

using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using MongoDB.Bson.Serialization.Attributes;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Infrastructure.Persistence.Documents;

public sealed class LocationDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<RoomDocument> Rooms { get; set; } = [];
    public int Version { get; set; }

    public static LocationDocument FromDomain(Location location)
    {
        return new LocationDocument
        {
            Id = location.Id.Value,
            Name = location.Name.Value,
            Street = location.Address.Street,
            City = location.Address.City,
            PostalCode = location.Address.PostalCode,
            Country = location.Address.Country,
            Rooms = location.Rooms.Select(RoomDocument.FromDomain).ToList(),
            Version = location.Version
        };
    }

    public Location ToDomain()
    {
        var location = DomainObjectMapper.CreateInstance<Location>();

        DomainObjectMapper.SetProperty(location, "Id", new LocationId(Id));
        DomainObjectMapper.SetProperty(location, "Name", new LocationName(Name));
        DomainObjectMapper.SetProperty(location, "Address",
            new Address(Street, City, PostalCode, Country));
        DomainObjectMapper.SetProperty(location, "Version", Version);

        var rooms = Rooms.Select(r => r.ToDomain());
        DomainObjectMapper.AddToList(location, "_rooms", rooms);

        return location;
    }
}

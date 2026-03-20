using System.Diagnostics.CodeAnalysis;
using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Facility.Domain.Entities;
using TrainingOrganizer.Facility.Domain.Events;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Domain;

public sealed class Location : AggregateRoot<LocationId>
{
    private readonly List<Room> _rooms = [];
    private LocationName _name;
    private Address _address;

    public required LocationName Name { get => _name; init => _name = value; }
    public required Address Address { get => _address; init => _address = value; }
    public IReadOnlyList<Room> Rooms => _rooms.AsReadOnly();

    [SetsRequiredMembers]
    private Location()
    {
        _name = default!;
        _address = default!;
    }

    public static Location Create(LocationName name, Address address)
    {
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstNull(address, nameof(address));

        return new Location
        {
            Id = LocationId.Create(),
            Name = name,
            Address = address
        };
    }

    public Room AddRoom(RoomName name, int capacity)
    {
        Guard.AgainstNull(name, nameof(name));

        if (_rooms.Any(r => r.Name == name))
            throw new BusinessRuleViolationException(
                "UniqueRoomName",
                $"A room named '{name}' already exists in this location.");

        var room = new Room(name, capacity);
        _rooms.Add(room);
        return room;
    }

    public void UpdateRoom(RoomId roomId, RoomName name, int capacity)
    {
        var room = GetRoom(roomId);

        if (_rooms.Any(r => r.Id != roomId && r.Name == name))
            throw new BusinessRuleViolationException(
                "UniqueRoomName",
                $"A room named '{name}' already exists in this location.");

        room.UpdateDetails(name, capacity);
    }

    public void EnableRoom(RoomId roomId)
    {
        var room = GetRoom(roomId);
        room.Enable();
    }

    public void DisableRoom(RoomId roomId)
    {
        var room = GetRoom(roomId);
        room.Disable();

        AddDomainEvent(new RoomDisabledEvent(Id, roomId, DateTimeOffset.UtcNow));
    }

    public void UpdateAddress(Address address)
    {
        Guard.AgainstNull(address, nameof(address));
        _address = address;
    }

    public void UpdateName(LocationName name)
    {
        Guard.AgainstNull(name, nameof(name));
        _name = name;
    }

    private Room GetRoom(RoomId roomId)
    {
        return _rooms.FirstOrDefault(r => r.Id == roomId)
               ?? throw new EntityNotFoundException(nameof(Room), roomId);
    }
}

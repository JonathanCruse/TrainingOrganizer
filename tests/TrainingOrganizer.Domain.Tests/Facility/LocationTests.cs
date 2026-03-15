using FluentAssertions;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.Enums;
using TrainingOrganizer.Domain.Facility.Events;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Domain.Tests.Facility;

public class LocationTests
{
    private static readonly LocationName DefaultName = new("Downtown Fitness Center");
    private static readonly Address DefaultAddress = new("123 Main St", "Berlin", "10115", "Germany");

    // --- Create ---

    [Fact]
    public void Create_ValidData_ReturnsLocationWithCorrectNameAndAddress()
    {
        var location = Location.Create(DefaultName, DefaultAddress);

        location.Id.Should().NotBeNull();
        location.Name.Should().Be(DefaultName);
        location.Address.Should().Be(DefaultAddress);
        location.Rooms.Should().BeEmpty();
    }

    // --- AddRoom ---

    [Fact]
    public void AddRoom_ValidName_AddsRoomWithGeneratedRoomId()
    {
        var location = Location.Create(DefaultName, DefaultAddress);
        var roomName = new RoomName("Main Hall");

        var room = location.AddRoom(roomName, 30);

        location.Rooms.Should().ContainSingle();
        room.Id.Should().NotBeNull();
        room.Name.Should().Be(roomName);
        room.Capacity.Should().Be(30);
        room.Status.Should().Be(RoomStatus.Enabled);
    }

    [Fact]
    public void AddRoom_DuplicateName_ThrowsBusinessRuleViolationException()
    {
        var location = Location.Create(DefaultName, DefaultAddress);
        var roomName = new RoomName("Room A");
        location.AddRoom(roomName, 20);

        var act = () => location.AddRoom(roomName, 15);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    // --- UpdateRoom ---

    [Fact]
    public void UpdateRoom_ValidData_UpdatesNameAndCapacity()
    {
        var location = Location.Create(DefaultName, DefaultAddress);
        var room = location.AddRoom(new RoomName("Small Room"), 10);
        var newName = new RoomName("Large Room");

        location.UpdateRoom(room.Id, newName, 50);

        var updated = location.Rooms.First(r => r.Id == room.Id);
        updated.Name.Should().Be(newName);
        updated.Capacity.Should().Be(50);
    }

    [Fact]
    public void UpdateRoom_DuplicateNameFromOtherRoom_ThrowsBusinessRuleViolationException()
    {
        var location = Location.Create(DefaultName, DefaultAddress);
        var room1 = location.AddRoom(new RoomName("Room A"), 20);
        var room2 = location.AddRoom(new RoomName("Room B"), 15);

        var act = () => location.UpdateRoom(room2.Id, new RoomName("Room A"), 15);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void UpdateRoom_SameNameOnSameRoom_Succeeds()
    {
        var location = Location.Create(DefaultName, DefaultAddress);
        var roomName = new RoomName("Room A");
        var room = location.AddRoom(roomName, 20);

        // Updating a room to keep its own name should not throw
        var act = () => location.UpdateRoom(room.Id, roomName, 25);

        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateRoom_NonExistentRoom_ThrowsEntityNotFoundException()
    {
        var location = Location.Create(DefaultName, DefaultAddress);

        var act = () => location.UpdateRoom(RoomId.Create(), new RoomName("Ghost"), 10);

        act.Should().Throw<EntityNotFoundException>();
    }

    // --- EnableRoom / DisableRoom ---

    [Fact]
    public void DisableRoom_EnabledRoom_DisablesRoom()
    {
        var location = Location.Create(DefaultName, DefaultAddress);
        var room = location.AddRoom(new RoomName("Room X"), 20);

        location.DisableRoom(room.Id);

        location.Rooms.First(r => r.Id == room.Id).Status.Should().Be(RoomStatus.Disabled);
    }

    [Fact]
    public void DisableRoom_RaisesRoomDisabledEvent()
    {
        var location = Location.Create(DefaultName, DefaultAddress);
        var room = location.AddRoom(new RoomName("Room Y"), 15);

        location.DisableRoom(room.Id);

        location.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoomDisabledEvent>()
            .Which.RoomId.Should().Be(room.Id);
    }

    [Fact]
    public void EnableRoom_DisabledRoom_EnablesRoom()
    {
        var location = Location.Create(DefaultName, DefaultAddress);
        var room = location.AddRoom(new RoomName("Room Z"), 25);
        location.DisableRoom(room.Id);

        location.EnableRoom(room.Id);

        location.Rooms.First(r => r.Id == room.Id).Status.Should().Be(RoomStatus.Enabled);
    }
}

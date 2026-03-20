using System.Diagnostics.CodeAnalysis;
using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Facility.Domain.Enums;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Domain.Entities;

public sealed class Room : Entity<RoomId>
{
    private RoomName _name;

    public required RoomName Name { get => _name; init => _name = value; }
    public int Capacity { get; private set; }
    public RoomStatus Status { get; private set; }

    [SetsRequiredMembers]
    private Room()
    {
        _name = default!;
    }

    [SetsRequiredMembers]
    internal Room(RoomName name, int capacity)
    {
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstNonPositive(capacity, nameof(capacity));

        Id = RoomId.Create();
        _name = name;
        Capacity = capacity;
        Status = RoomStatus.Enabled;
    }

    public void UpdateDetails(RoomName name, int capacity)
    {
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstNonPositive(capacity, nameof(capacity));

        _name = name;
        Capacity = capacity;
    }

    public void Enable() => Status = RoomStatus.Enabled;

    public void Disable() => Status = RoomStatus.Disabled;

    public bool IsEnabled => Status == RoomStatus.Enabled;
}

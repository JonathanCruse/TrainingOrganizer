using TrainingOrganizer.Shared.Enums;

namespace TrainingOrganizer.Shared.Models;

public sealed record LocationResponse(
    Guid Id,
    string Name,
    string Street,
    string City,
    string PostalCode,
    string Country,
    IReadOnlyList<RoomResponse> Rooms);

public sealed record RoomResponse(
    Guid Id,
    string Name,
    int Capacity,
    RoomStatus Status);

public sealed record BookingResponse(
    Guid Id,
    Guid RoomId,
    Guid LocationId,
    DateTimeOffset Start,
    DateTimeOffset End,
    BookingStatus Status,
    BookingReferenceType ReferenceType,
    Guid ReferenceId,
    DateTimeOffset CreatedAt,
    Guid CreatedBy);

public sealed record TimeSlotResponse(DateTimeOffset Start, DateTimeOffset End);

public sealed record CreateLocationRequest(
    string Name,
    string Street,
    string City,
    string PostalCode,
    string Country);

public sealed record UpdateLocationRequest(
    string Name,
    string Street,
    string City,
    string PostalCode,
    string Country);

public sealed record AddRoomRequest(string Name, int Capacity);
public sealed record UpdateRoomRequest(string Name, int Capacity);

public sealed record CreateBookingRequest(
    Guid RoomId,
    Guid LocationId,
    DateTimeOffset Start,
    DateTimeOffset End,
    string ReferenceType,
    Guid ReferenceId);

public sealed record RescheduleBookingRequest(DateTimeOffset Start, DateTimeOffset End);

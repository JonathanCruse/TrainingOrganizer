namespace TrainingOrganizer.Api.Contracts;

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

public sealed record AddRoomRequest(
    string Name,
    int Capacity);

public sealed record UpdateRoomRequest(
    string Name,
    int Capacity);

public sealed record CreateBookingRequest(
    Guid RoomId,
    Guid LocationId,
    DateTimeOffset Start,
    DateTimeOffset End,
    string ReferenceType,
    Guid ReferenceId);

public sealed record RescheduleBookingRequest(
    DateTimeOffset Start,
    DateTimeOffset End);

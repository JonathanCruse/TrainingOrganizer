using MongoDB.Bson.Serialization.Attributes;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.Enums;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Infrastructure.Persistence.Documents;

public sealed class BookingDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }
    public Guid LocationId { get; set; }
    public DateTimeOffset TimeSlotStart { get; set; }
    public DateTimeOffset TimeSlotEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public int Version { get; set; }

    public static BookingDocument FromDomain(Booking booking)
    {
        return new BookingDocument
        {
            Id = booking.Id.Value,
            RoomId = booking.RoomId.Value,
            LocationId = booking.LocationId.Value,
            TimeSlotStart = booking.TimeSlot.Start,
            TimeSlotEnd = booking.TimeSlot.End,
            Status = booking.Status.ToString(),
            ReferenceType = booking.Reference.ReferenceType.ToString(),
            ReferenceId = booking.Reference.ReferenceId,
            CreatedAt = booking.CreatedAt,
            CreatedBy = booking.CreatedBy,
            Version = booking.Version
        };
    }

    public Booking ToDomain()
    {
        var booking = DomainObjectMapper.CreateInstance<Booking>();

        DomainObjectMapper.SetProperty(booking, "Id", new BookingId(Id));
        DomainObjectMapper.SetProperty(booking, "RoomId", new RoomId(RoomId));
        DomainObjectMapper.SetProperty(booking, "LocationId", new LocationId(LocationId));
        DomainObjectMapper.SetProperty(booking, "TimeSlot",
            new TimeSlot(TimeSlotStart, TimeSlotEnd));
        DomainObjectMapper.SetProperty(booking, "Status",
            Enum.Parse<BookingStatus>(Status));
        DomainObjectMapper.SetProperty(booking, "Reference",
            new BookingReference(
                Enum.Parse<BookingReferenceType>(ReferenceType),
                ReferenceId));
        DomainObjectMapper.SetProperty(booking, "CreatedAt", CreatedAt);
        DomainObjectMapper.SetProperty(booking, "CreatedBy", CreatedBy);
        DomainObjectMapper.SetProperty(booking, "Version", Version);

        return booking;
    }
}

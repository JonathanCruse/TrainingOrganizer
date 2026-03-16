using MongoDB.Bson.Serialization.Attributes;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Infrastructure.Persistence.Documents;

public sealed class TrainingDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset TimeSlotStart { get; set; }
    public DateTimeOffset TimeSlotEnd { get; set; }
    public int CapacityMin { get; set; }
    public int CapacityMax { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ParticipantDocument> Participants { get; set; } = [];
    public List<Guid> TrainerIds { get; set; } = [];
    public List<RoomRequirementDocument> RoomRequirements { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public int Version { get; set; }

    public static TrainingDocument FromDomain(Domain.Training.Training training)
    {
        return new TrainingDocument
        {
            Id = training.Id.Value,
            Title = training.Title.Value,
            Description = training.Description.Value,
            TimeSlotStart = training.TimeSlot.Start,
            TimeSlotEnd = training.TimeSlot.End,
            CapacityMin = training.Capacity.Min,
            CapacityMax = training.Capacity.Max,
            Visibility = training.Visibility.ToString(),
            Status = training.Status.ToString(),
            Participants = training.Participants
                .Select(ParticipantDocument.FromDomain).ToList(),
            TrainerIds = training.TrainerIds.Select(t => t.Value).ToList(),
            RoomRequirements = training.RoomRequirements
                .Select(RoomRequirementDocument.FromDomain).ToList(),
            CreatedAt = training.CreatedAt,
            CreatedBy = training.CreatedBy.Value,
            Version = training.Version
        };
    }

    public Domain.Training.Training ToDomain()
    {
        var training = DomainObjectMapper.CreateInstance<Domain.Training.Training>();

        DomainObjectMapper.SetProperty(training, "Id", new TrainingId(Id));
        DomainObjectMapper.SetProperty(training, "Title", new TrainingTitle(Title));
        DomainObjectMapper.SetProperty(training, "Description", new TrainingDescription(Description));
        DomainObjectMapper.SetProperty(training, "TimeSlot", new TimeSlot(TimeSlotStart, TimeSlotEnd));
        DomainObjectMapper.SetProperty(training, "Capacity", new Capacity(CapacityMin, CapacityMax));
        DomainObjectMapper.SetProperty(training, "Visibility", Enum.Parse<Visibility>(this.Visibility));
        DomainObjectMapper.SetProperty(training, "Status", Enum.Parse<TrainingStatus>(Status));
        DomainObjectMapper.SetProperty(training, "CreatedAt", CreatedAt);
        DomainObjectMapper.SetProperty(training, "CreatedBy", new MemberId(CreatedBy));
        DomainObjectMapper.SetProperty(training, "Version", Version);

        var participants = Participants.Select(p => p.ToDomain());
        DomainObjectMapper.AddToList(training, "_participants", participants);

        var trainerIds = TrainerIds.Select(t => new MemberId(t));
        DomainObjectMapper.AddToList(training, "_trainerIds", trainerIds);

        var roomRequirements = RoomRequirements.Select(r => r.ToDomain());
        DomainObjectMapper.AddToList(training, "_roomRequirements", roomRequirements);

        return training;
    }
}

using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using MongoDB.Bson.Serialization.Attributes;
using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Infrastructure.Persistence.Documents;

public sealed class TrainingSessionDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public Guid RecurringTrainingId { get; set; }
    public string EffectiveTitle { get; set; } = string.Empty;
    public string EffectiveDescription { get; set; } = string.Empty;
    public DateTimeOffset TimeSlotStart { get; set; }
    public DateTimeOffset TimeSlotEnd { get; set; }
    public int EffectiveCapacityMin { get; set; }
    public int EffectiveCapacityMax { get; set; }
    public string EffectiveVisibility { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public SessionOverridesDocument Overrides { get; set; } = new();
    public List<ParticipantDocument> Participants { get; set; } = [];
    public List<Guid> EffectiveTrainerIds { get; set; } = [];
    public List<RoomRequirementDocument> EffectiveRoomRequirements { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public int Version { get; set; }

    public static TrainingSessionDocument FromDomain(TrainingSession session)
    {
        return new TrainingSessionDocument
        {
            Id = session.Id.Value,
            RecurringTrainingId = session.RecurringTrainingId.Value,
            EffectiveTitle = session.EffectiveTitle.Value,
            EffectiveDescription = session.EffectiveDescription.Value,
            TimeSlotStart = session.TimeSlot.Start,
            TimeSlotEnd = session.TimeSlot.End,
            EffectiveCapacityMin = session.EffectiveCapacity.Min,
            EffectiveCapacityMax = session.EffectiveCapacity.Max,
            EffectiveVisibility = session.EffectiveVisibility.ToString(),
            Status = session.Status.ToString(),
            Overrides = SessionOverridesDocument.FromDomain(session.Overrides),
            Participants = session.Participants
                .Select(ParticipantDocument.FromDomain).ToList(),
            EffectiveTrainerIds = session.EffectiveTrainerIds.Select(t => t.Value).ToList(),
            EffectiveRoomRequirements = session.EffectiveRoomRequirements
                .Select(RoomRequirementDocument.FromDomain).ToList(),
            CreatedAt = session.CreatedAt,
            Version = session.Version
        };
    }

    public TrainingSession ToDomain()
    {
        var session = DomainObjectMapper.CreateInstance<TrainingSession>();

        DomainObjectMapper.SetProperty(session, "Id", new TrainingSessionId(Id));
        DomainObjectMapper.SetProperty(session, "RecurringTrainingId",
            new RecurringTrainingId(RecurringTrainingId));
        DomainObjectMapper.SetProperty(session, "EffectiveTitle",
            new TrainingTitle(EffectiveTitle));
        DomainObjectMapper.SetProperty(session, "EffectiveDescription",
            new TrainingDescription(EffectiveDescription));
        DomainObjectMapper.SetProperty(session, "TimeSlot",
            new TimeSlot(TimeSlotStart, TimeSlotEnd));
        DomainObjectMapper.SetProperty(session, "EffectiveCapacity",
            new Capacity(EffectiveCapacityMin, EffectiveCapacityMax));
        DomainObjectMapper.SetProperty(session, "EffectiveVisibility",
            Enum.Parse<Visibility>(EffectiveVisibility));
        DomainObjectMapper.SetProperty(session, "Status",
            Enum.Parse<SessionStatus>(Status));
        DomainObjectMapper.SetProperty(session, "Overrides", Overrides.ToDomain());
        DomainObjectMapper.SetProperty(session, "CreatedAt", CreatedAt);
        DomainObjectMapper.SetProperty(session, "Version", Version);

        var participants = Participants.Select(p => p.ToDomain());
        DomainObjectMapper.AddToList(session, "_participants", participants);

        var trainerIds = EffectiveTrainerIds.Select(t => new MemberId(t));
        DomainObjectMapper.AddToList(session, "_effectiveTrainerIds", trainerIds);

        var roomRequirements = EffectiveRoomRequirements.Select(r => r.ToDomain());
        DomainObjectMapper.AddToList(session, "_effectiveRoomRequirements", roomRequirements);

        return session;
    }
}

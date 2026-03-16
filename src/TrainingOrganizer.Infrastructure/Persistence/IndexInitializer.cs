using MongoDB.Driver;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Persistence;

public sealed class IndexInitializer
{
    public async Task InitializeAsync(MongoDbContext context, CancellationToken ct = default)
    {
        await CreateMemberIndexes(context, ct);
        await CreateTrainingIndexes(context, ct);
        await CreateTrainingSessionIndexes(context, ct);
        await CreateBookingIndexes(context, ct);
        await CreateLocationIndexes(context, ct);
    }

    private static async Task CreateMemberIndexes(MongoDbContext context, CancellationToken ct)
    {
        var collection = context.Members;

        var emailIndex = new CreateIndexModel<MemberDocument>(
            Builders<MemberDocument>.IndexKeys.Ascending(d => d.Email),
            new CreateIndexOptions { Unique = true, Name = "IX_Members_Email" });

        var externalIdentityIndex = new CreateIndexModel<MemberDocument>(
            Builders<MemberDocument>.IndexKeys
                .Ascending(d => d.ExternalIdentityProvider)
                .Ascending(d => d.ExternalIdentitySubjectId),
            new CreateIndexOptions { Unique = true, Name = "IX_Members_ExternalIdentity" });

        var statusIndex = new CreateIndexModel<MemberDocument>(
            Builders<MemberDocument>.IndexKeys.Ascending(d => d.RegistrationStatus),
            new CreateIndexOptions { Name = "IX_Members_RegistrationStatus" });

        await collection.Indexes.CreateManyAsync(
            [emailIndex, externalIdentityIndex, statusIndex],
            ct);
    }

    private static async Task CreateTrainingIndexes(MongoDbContext context, CancellationToken ct)
    {
        var collection = context.Trainings;

        var statusStartIndex = new CreateIndexModel<TrainingDocument>(
            Builders<TrainingDocument>.IndexKeys
                .Ascending(d => d.Status)
                .Ascending(d => d.TimeSlotStart),
            new CreateIndexOptions { Name = "IX_Trainings_Status_Start" });

        var trainerIndex = new CreateIndexModel<TrainingDocument>(
            Builders<TrainingDocument>.IndexKeys.Ascending(d => d.TrainerIds),
            new CreateIndexOptions { Name = "IX_Trainings_TrainerIds" });

        var participantIndex = new CreateIndexModel<TrainingDocument>(
            Builders<TrainingDocument>.IndexKeys.Ascending("Participants.MemberId"),
            new CreateIndexOptions { Name = "IX_Trainings_Participants_MemberId" });

        await collection.Indexes.CreateManyAsync(
            [statusStartIndex, trainerIndex, participantIndex],
            ct);
    }

    private static async Task CreateTrainingSessionIndexes(MongoDbContext context, CancellationToken ct)
    {
        var collection = context.TrainingSessions;

        var recurringStartIndex = new CreateIndexModel<TrainingSessionDocument>(
            Builders<TrainingSessionDocument>.IndexKeys
                .Ascending(d => d.RecurringTrainingId)
                .Ascending(d => d.TimeSlotStart),
            new CreateIndexOptions { Name = "IX_Sessions_RecurringId_Start" });

        var statusStartIndex = new CreateIndexModel<TrainingSessionDocument>(
            Builders<TrainingSessionDocument>.IndexKeys
                .Ascending(d => d.Status)
                .Ascending(d => d.TimeSlotStart),
            new CreateIndexOptions { Name = "IX_Sessions_Status_Start" });

        var participantIndex = new CreateIndexModel<TrainingSessionDocument>(
            Builders<TrainingSessionDocument>.IndexKeys.Ascending("Participants.MemberId"),
            new CreateIndexOptions { Name = "IX_Sessions_Participants_MemberId" });

        await collection.Indexes.CreateManyAsync(
            [recurringStartIndex, statusStartIndex, participantIndex],
            ct);
    }

    private static async Task CreateBookingIndexes(MongoDbContext context, CancellationToken ct)
    {
        var collection = context.Bookings;

        var roomConflictIndex = new CreateIndexModel<BookingDocument>(
            Builders<BookingDocument>.IndexKeys
                .Ascending(d => d.RoomId)
                .Ascending(d => d.Status)
                .Ascending(d => d.TimeSlotStart)
                .Ascending(d => d.TimeSlotEnd),
            new CreateIndexOptions { Name = "IX_Bookings_Room_Status_TimeSlot" });

        var referenceIndex = new CreateIndexModel<BookingDocument>(
            Builders<BookingDocument>.IndexKeys
                .Ascending(d => d.ReferenceType)
                .Ascending(d => d.ReferenceId),
            new CreateIndexOptions { Name = "IX_Bookings_Reference" });

        await collection.Indexes.CreateManyAsync(
            [roomConflictIndex, referenceIndex],
            ct);
    }

    private static async Task CreateLocationIndexes(MongoDbContext context, CancellationToken ct)
    {
        var collection = context.Locations;

        var nameIndex = new CreateIndexModel<LocationDocument>(
            Builders<LocationDocument>.IndexKeys.Ascending(d => d.Name),
            new CreateIndexOptions { Unique = true, Name = "IX_Locations_Name" });

        await collection.Indexes.CreateManyAsync([nameIndex], ct);
    }
}

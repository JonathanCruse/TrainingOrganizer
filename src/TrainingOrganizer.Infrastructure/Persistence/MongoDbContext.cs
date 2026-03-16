using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<MemberDocument> Members =>
        _database.GetCollection<MemberDocument>("members");

    public IMongoCollection<TrainingDocument> Trainings =>
        _database.GetCollection<TrainingDocument>("trainings");

    public IMongoCollection<RecurringTrainingDocument> RecurringTrainings =>
        _database.GetCollection<RecurringTrainingDocument>("recurring_trainings");

    public IMongoCollection<TrainingSessionDocument> TrainingSessions =>
        _database.GetCollection<TrainingSessionDocument>("training_sessions");

    public IMongoCollection<LocationDocument> Locations =>
        _database.GetCollection<LocationDocument>("locations");

    public IMongoCollection<BookingDocument> Bookings =>
        _database.GetCollection<BookingDocument>("bookings");
}

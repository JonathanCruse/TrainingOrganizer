using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TrainingOrganizer.SharedKernel.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        Database = client.GetDatabase(settings.Value.DatabaseName);
    }
}

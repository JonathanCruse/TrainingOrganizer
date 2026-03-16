namespace TrainingOrganizer.Infrastructure.Persistence;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDB";
    public string ConnectionString { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = "training-organizer";
}

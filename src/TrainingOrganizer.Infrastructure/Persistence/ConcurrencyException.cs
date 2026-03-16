namespace TrainingOrganizer.Infrastructure.Persistence;

public sealed class ConcurrencyException : Exception
{
    public string EntityName { get; }
    public object EntityId { get; }

    public ConcurrencyException(string entityName, object entityId)
        : base($"Concurrency conflict for {entityName} with ID '{entityId}'. The document was modified by another process.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

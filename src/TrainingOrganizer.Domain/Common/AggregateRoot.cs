namespace TrainingOrganizer.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : StronglyTypedId
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public int Version { get; protected set; }

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

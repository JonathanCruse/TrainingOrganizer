using TrainingOrganizer.SharedKernel.Domain.Exceptions;

namespace TrainingOrganizer.SharedKernel.Domain;

public abstract record StronglyTypedId
{
    public Guid Value { get; }

    protected StronglyTypedId(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException($"{GetType().Name} cannot be empty.");

        Value = value;
    }

    public override string ToString() => Value.ToString();
}

using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Membership.ValueObjects;

public sealed record PersonName : ValueObject
{
    public const int MaxLength = 100;

    public string FirstName { get; }
    public string LastName { get; }

    public PersonName(string firstName, string lastName)
    {
        FirstName = Guard.AgainstOverflow(
            Guard.AgainstNullOrWhiteSpace(firstName, nameof(firstName)),
            MaxLength, nameof(firstName));
        LastName = Guard.AgainstOverflow(
            Guard.AgainstNullOrWhiteSpace(lastName, nameof(lastName)),
            MaxLength, nameof(lastName));
    }

    public override string ToString() => $"{FirstName} {LastName}";
}

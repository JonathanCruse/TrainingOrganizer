using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Membership.Domain.ValueObjects;

public sealed record ExternalIdentity : ValueObject
{
    public string Provider { get; }
    public string SubjectId { get; }

    public ExternalIdentity(string provider, string subjectId)
    {
        Provider = Guard.AgainstNullOrWhiteSpace(provider, nameof(provider));
        SubjectId = Guard.AgainstNullOrWhiteSpace(subjectId, nameof(subjectId));
    }
}

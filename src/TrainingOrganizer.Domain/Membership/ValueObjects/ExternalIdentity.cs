using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Membership.ValueObjects;

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

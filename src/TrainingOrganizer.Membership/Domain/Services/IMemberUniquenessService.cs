using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Domain.Services;

public interface IMemberUniquenessService
{
    Task<bool> IsEmailUniqueAsync(
        Email email,
        MemberId? excludeMemberId = null,
        CancellationToken cancellationToken = default);
}

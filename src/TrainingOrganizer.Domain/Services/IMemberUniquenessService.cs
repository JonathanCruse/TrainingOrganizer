using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Services;

public interface IMemberUniquenessService
{
    Task<bool> IsEmailUniqueAsync(
        Email email,
        MemberId? excludeMemberId = null,
        CancellationToken cancellationToken = default);
}

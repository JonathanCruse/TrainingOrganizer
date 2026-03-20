using TrainingOrganizer.SharedKernel.Application.Interfaces;

namespace TrainingOrganizer.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// MongoDB operations are immediately persisted at the document level,
/// so this is effectively a no-op. Retained for interface compliance and
/// as a placeholder for future multi-document transaction support.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }
}

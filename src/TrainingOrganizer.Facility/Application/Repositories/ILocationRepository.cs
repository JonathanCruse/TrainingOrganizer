using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Application.Repositories;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(LocationId id, CancellationToken ct = default);
    Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken ct = default);
    Task<PagedList<Location>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Location location, CancellationToken ct = default);
    Task UpdateAsync(Location location, CancellationToken ct = default);
}

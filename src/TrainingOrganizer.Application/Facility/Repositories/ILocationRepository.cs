using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Application.Facility.Repositories;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(LocationId id, CancellationToken ct = default);
    Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken ct = default);
    Task<PagedList<Location>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Location location, CancellationToken ct = default);
    Task UpdateAsync(Location location, CancellationToken ct = default);
}

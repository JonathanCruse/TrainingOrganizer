using MongoDB.Driver;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.ValueObjects;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Persistence.Repositories;

public sealed class RecurringTrainingRepository : IRecurringTrainingRepository
{
    private readonly MongoDbContext _context;

    public RecurringTrainingRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<RecurringTraining?> GetByIdAsync(RecurringTrainingId id, CancellationToken ct = default)
    {
        var filter = Builders<RecurringTrainingDocument>.Filter.Eq(d => d.Id, id.Value);
        var document = await _context.RecurringTrainings.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<PagedList<RecurringTraining>> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var filter = Builders<RecurringTrainingDocument>.Filter.Empty;

        var totalCount = await _context.RecurringTrainings.CountDocumentsAsync(filter, cancellationToken: ct);
        var documents = await _context.RecurringTrainings
            .Find(filter)
            .Sort(Builders<RecurringTrainingDocument>.Sort.Descending(d => d.CreatedAt))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        var items = documents.Select(d => d.ToDomain()).ToList();
        return new PagedList<RecurringTraining>(items, page, pageSize, (int)totalCount);
    }

    public async Task<IReadOnlyList<RecurringTraining>> GetActiveAsync(CancellationToken ct = default)
    {
        var filter = Builders<RecurringTrainingDocument>.Filter
            .Eq(d => d.Status, RecurringTrainingStatus.Active.ToString());

        var documents = await _context.RecurringTrainings
            .Find(filter)
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task AddAsync(RecurringTraining recurringTraining, CancellationToken ct = default)
    {
        var document = RecurringTrainingDocument.FromDomain(recurringTraining);
        await _context.RecurringTrainings.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(RecurringTraining recurringTraining, CancellationToken ct = default)
    {
        var document = RecurringTrainingDocument.FromDomain(recurringTraining);
        var expectedVersion = document.Version;
        document.Version = expectedVersion + 1;

        var filter = Builders<RecurringTrainingDocument>.Filter.And(
            Builders<RecurringTrainingDocument>.Filter.Eq(d => d.Id, document.Id),
            Builders<RecurringTrainingDocument>.Filter.Eq(d => d.Version, expectedVersion));

        var result = await _context.RecurringTrainings.ReplaceOneAsync(filter, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new ConcurrencyException(nameof(RecurringTraining), recurringTraining.Id);
    }
}

using MongoDB.Driver;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;
using TrainingOrganizer.Training.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Training.Infrastructure.Persistence.Repositories;

public sealed class RecurringTrainingRepository : IRecurringTrainingRepository
{
    private readonly MongoDbContext _context;

    private IMongoCollection<RecurringTrainingDocument> RecurringTrainings => _context.Database.GetCollection<RecurringTrainingDocument>("recurring_trainings");

    public RecurringTrainingRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<RecurringTraining?> GetByIdAsync(RecurringTrainingId id, CancellationToken ct = default)
    {
        var filter = Builders<RecurringTrainingDocument>.Filter.Eq(d => d.Id, id.Value);
        var document = await RecurringTrainings.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<PagedList<RecurringTraining>> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var filter = Builders<RecurringTrainingDocument>.Filter.Empty;

        var totalCount = await RecurringTrainings.CountDocumentsAsync(filter, cancellationToken: ct);
        var documents = await RecurringTrainings
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

        var documents = await RecurringTrainings
            .Find(filter)
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task AddAsync(RecurringTraining recurringTraining, CancellationToken ct = default)
    {
        var document = RecurringTrainingDocument.FromDomain(recurringTraining);
        await RecurringTrainings.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(RecurringTraining recurringTraining, CancellationToken ct = default)
    {
        var document = RecurringTrainingDocument.FromDomain(recurringTraining);
        var expectedVersion = document.Version;
        document.Version = expectedVersion + 1;

        var filter = Builders<RecurringTrainingDocument>.Filter.And(
            Builders<RecurringTrainingDocument>.Filter.Eq(d => d.Id, document.Id),
            Builders<RecurringTrainingDocument>.Filter.Eq(d => d.Version, expectedVersion));

        var result = await RecurringTrainings.ReplaceOneAsync(filter, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new ConcurrencyException(nameof(RecurringTraining), recurringTraining.Id);
    }
}

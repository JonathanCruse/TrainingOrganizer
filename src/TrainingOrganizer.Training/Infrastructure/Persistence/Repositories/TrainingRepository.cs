using MongoDB.Driver;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;
using TrainingOrganizer.Training.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Training.Infrastructure.Persistence.Repositories;

public sealed class TrainingRepository : ITrainingRepository
{
    private readonly MongoDbContext _context;

    private IMongoCollection<TrainingDocument> Trainings => _context.Database.GetCollection<TrainingDocument>("trainings");

    public TrainingRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Training?> GetByIdAsync(TrainingId id, CancellationToken ct = default)
    {
        var filter = Builders<TrainingDocument>.Filter.Eq(d => d.Id, id.Value);
        var document = await Trainings.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<PagedList<Domain.Training>> GetPagedAsync(
        int page, int pageSize, TrainingStatus? statusFilter, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var filterBuilder = Builders<TrainingDocument>.Filter;
        var filter = filterBuilder.Empty;

        if (statusFilter.HasValue)
        {
            filter &= filterBuilder.Eq(d => d.Status, statusFilter.Value.ToString());
        }

        if (from.HasValue)
        {
            filter &= filterBuilder.Gte(d => d.TimeSlotStart, from.Value);
        }

        if (to.HasValue)
        {
            filter &= filterBuilder.Lte(d => d.TimeSlotStart, to.Value);
        }

        var totalCount = await Trainings.CountDocumentsAsync(filter, cancellationToken: ct);
        var documents = await Trainings
            .Find(filter)
            .Sort(Builders<TrainingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        var items = documents.Select(d => d.ToDomain()).ToList();
        return new PagedList<Domain.Training>(items, page, pageSize, (int)totalCount);
    }

    public async Task<IReadOnlyList<Domain.Training>> GetByMemberParticipationAsync(
        MemberId memberId, CancellationToken ct = default)
    {
        var filter = Builders<TrainingDocument>.Filter.ElemMatch(
            d => d.Participants,
            p => p.MemberId == memberId.Value && p.Status != ParticipationStatus.Canceled.ToString());

        var documents = await Trainings
            .Find(filter)
            .Sort(Builders<TrainingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Domain.Training>> GetByTrainerAsync(
        MemberId trainerId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var filterBuilder = Builders<TrainingDocument>.Filter;
        var filter = filterBuilder.AnyEq(d => d.TrainerIds, trainerId.Value);

        if (from.HasValue)
        {
            filter &= filterBuilder.Gte(d => d.TimeSlotStart, from.Value);
        }

        if (to.HasValue)
        {
            filter &= filterBuilder.Lte(d => d.TimeSlotStart, to.Value);
        }

        var documents = await Trainings
            .Find(filter)
            .Sort(Builders<TrainingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task AddAsync(Domain.Training training, CancellationToken ct = default)
    {
        var document = TrainingDocument.FromDomain(training);
        await Trainings.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(Domain.Training training, CancellationToken ct = default)
    {
        var document = TrainingDocument.FromDomain(training);
        var expectedVersion = document.Version;
        document.Version = expectedVersion + 1;

        var filter = Builders<TrainingDocument>.Filter.And(
            Builders<TrainingDocument>.Filter.Eq(d => d.Id, document.Id),
            Builders<TrainingDocument>.Filter.Eq(d => d.Version, expectedVersion));

        var result = await Trainings.ReplaceOneAsync(filter, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new ConcurrencyException(nameof(Domain.Training), training.Id);
    }
}

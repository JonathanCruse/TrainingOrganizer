using MongoDB.Driver;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.ValueObjects;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Persistence.Repositories;

public sealed class TrainingRepository : ITrainingRepository
{
    private readonly MongoDbContext _context;

    public TrainingRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Training.Training?> GetByIdAsync(TrainingId id, CancellationToken ct = default)
    {
        var filter = Builders<TrainingDocument>.Filter.Eq(d => d.Id, id.Value);
        var document = await _context.Trainings.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<PagedList<Domain.Training.Training>> GetPagedAsync(
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

        var totalCount = await _context.Trainings.CountDocumentsAsync(filter, cancellationToken: ct);
        var documents = await _context.Trainings
            .Find(filter)
            .Sort(Builders<TrainingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        var items = documents.Select(d => d.ToDomain()).ToList();
        return new PagedList<Domain.Training.Training>(items, page, pageSize, (int)totalCount);
    }

    public async Task<IReadOnlyList<Domain.Training.Training>> GetByMemberParticipationAsync(
        MemberId memberId, CancellationToken ct = default)
    {
        var filter = Builders<TrainingDocument>.Filter.ElemMatch(
            d => d.Participants,
            p => p.MemberId == memberId.Value && p.Status != ParticipationStatus.Canceled.ToString());

        var documents = await _context.Trainings
            .Find(filter)
            .Sort(Builders<TrainingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Domain.Training.Training>> GetByTrainerAsync(
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

        var documents = await _context.Trainings
            .Find(filter)
            .Sort(Builders<TrainingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task AddAsync(Domain.Training.Training training, CancellationToken ct = default)
    {
        var document = TrainingDocument.FromDomain(training);
        await _context.Trainings.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(Domain.Training.Training training, CancellationToken ct = default)
    {
        var document = TrainingDocument.FromDomain(training);
        var expectedVersion = document.Version;
        document.Version = expectedVersion + 1;

        var filter = Builders<TrainingDocument>.Filter.And(
            Builders<TrainingDocument>.Filter.Eq(d => d.Id, document.Id),
            Builders<TrainingDocument>.Filter.Eq(d => d.Version, expectedVersion));

        var result = await _context.Trainings.ReplaceOneAsync(filter, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new ConcurrencyException(nameof(Domain.Training.Training), training.Id);
    }
}

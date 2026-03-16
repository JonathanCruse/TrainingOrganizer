using MongoDB.Driver;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.ValueObjects;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Persistence.Repositories;

public sealed class TrainingSessionRepository : ITrainingSessionRepository
{
    private readonly MongoDbContext _context;

    public TrainingSessionRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<TrainingSession?> GetByIdAsync(TrainingSessionId id, CancellationToken ct = default)
    {
        var filter = Builders<TrainingSessionDocument>.Filter.Eq(d => d.Id, id.Value);
        var document = await _context.TrainingSessions.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<IReadOnlyList<TrainingSession>> GetByRecurringTrainingIdAsync(
        RecurringTrainingId recurringTrainingId, CancellationToken ct = default)
    {
        var filter = Builders<TrainingSessionDocument>.Filter
            .Eq(d => d.RecurringTrainingId, recurringTrainingId.Value);

        var documents = await _context.TrainingSessions
            .Find(filter)
            .Sort(Builders<TrainingSessionDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task<PagedList<TrainingSession>> GetPagedAsync(
        int page, int pageSize, RecurringTrainingId? recurringTrainingId,
        DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var filterBuilder = Builders<TrainingSessionDocument>.Filter;
        var filter = filterBuilder.Empty;

        if (recurringTrainingId is not null)
        {
            filter &= filterBuilder.Eq(d => d.RecurringTrainingId, recurringTrainingId.Value);
        }

        if (from.HasValue)
        {
            filter &= filterBuilder.Gte(d => d.TimeSlotStart, from.Value);
        }

        if (to.HasValue)
        {
            filter &= filterBuilder.Lte(d => d.TimeSlotStart, to.Value);
        }

        var totalCount = await _context.TrainingSessions.CountDocumentsAsync(filter, cancellationToken: ct);
        var documents = await _context.TrainingSessions
            .Find(filter)
            .Sort(Builders<TrainingSessionDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        var items = documents.Select(d => d.ToDomain()).ToList();
        return new PagedList<TrainingSession>(items, page, pageSize, (int)totalCount);
    }

    public async Task<IReadOnlyList<TrainingSession>> GetByMemberParticipationAsync(
        MemberId memberId, CancellationToken ct = default)
    {
        var filter = Builders<TrainingSessionDocument>.Filter.ElemMatch(
            d => d.Participants,
            p => p.MemberId == memberId.Value && p.Status != ParticipationStatus.Canceled.ToString());

        var documents = await _context.TrainingSessions
            .Find(filter)
            .Sort(Builders<TrainingSessionDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task AddAsync(TrainingSession session, CancellationToken ct = default)
    {
        var document = TrainingSessionDocument.FromDomain(session);
        await _context.TrainingSessions.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task AddRangeAsync(IEnumerable<TrainingSession> sessions, CancellationToken ct = default)
    {
        var documents = sessions.Select(TrainingSessionDocument.FromDomain).ToList();
        if (documents.Count > 0)
        {
            await _context.TrainingSessions.InsertManyAsync(documents, cancellationToken: ct);
        }
    }

    public async Task UpdateAsync(TrainingSession session, CancellationToken ct = default)
    {
        var document = TrainingSessionDocument.FromDomain(session);
        var expectedVersion = document.Version;
        document.Version = expectedVersion + 1;

        var filter = Builders<TrainingSessionDocument>.Filter.And(
            Builders<TrainingSessionDocument>.Filter.Eq(d => d.Id, document.Id),
            Builders<TrainingSessionDocument>.Filter.Eq(d => d.Version, expectedVersion));

        var result = await _context.TrainingSessions.ReplaceOneAsync(filter, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new ConcurrencyException(nameof(TrainingSession), session.Id);
    }
}

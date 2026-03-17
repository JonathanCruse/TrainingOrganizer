using MongoDB.Driver;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Membership.Repositories;
using TrainingOrganizer.Domain.Membership;
using TrainingOrganizer.Domain.Membership.Enums;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Persistence.Repositories;

public sealed class MemberRepository : IMemberRepository
{
    private readonly MongoDbContext _context;

    public MemberRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByIdAsync(MemberId id, CancellationToken ct = default)
    {
        var filter = Builders<MemberDocument>.Filter.Eq(d => d.Id, id.Value);
        var document = await _context.Members.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<Member?> GetByEmailAsync(Email email, CancellationToken ct = default)
    {
        var filter = Builders<MemberDocument>.Filter.Eq(d => d.Email, email.Value);
        var document = await _context.Members.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<Member?> GetByExternalIdentityAsync(string provider, string subjectId, CancellationToken ct = default)
    {
        var filter = Builders<MemberDocument>.Filter.And(
            Builders<MemberDocument>.Filter.Eq(d => d.ExternalIdentityProvider, provider),
            Builders<MemberDocument>.Filter.Eq(d => d.ExternalIdentitySubjectId, subjectId));
        var document = await _context.Members.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<PagedList<Member>> GetPagedAsync(
        int page, int pageSize, RegistrationStatus? statusFilter, string? searchTerm, CancellationToken ct = default)
    {
        var filterBuilder = Builders<MemberDocument>.Filter;
        var filter = filterBuilder.Empty;

        if (statusFilter.HasValue)
        {
            filter &= filterBuilder.Eq(d => d.RegistrationStatus, statusFilter.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(d => d.FirstName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                filterBuilder.Regex(d => d.LastName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                filterBuilder.Regex(d => d.Email, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")));
            filter &= searchFilter;
        }

        var totalCount = await _context.Members.CountDocumentsAsync(filter, cancellationToken: ct);
        var documents = await _context.Members
            .Find(filter)
            .Sort(Builders<MemberDocument>.Sort.Ascending(d => d.LastName).Ascending(d => d.FirstName))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        var items = documents.Select(d => d.ToDomain()).ToList();
        return new PagedList<Member>(items, page, pageSize, (int)totalCount);
    }

    public async Task<List<Member>> GetTrainersAsync(CancellationToken ct = default)
    {
        var filter = Builders<MemberDocument>.Filter.And(
            Builders<MemberDocument>.Filter.Eq(d => d.RegistrationStatus, RegistrationStatus.Approved.ToString()),
            Builders<MemberDocument>.Filter.Or(
                Builders<MemberDocument>.Filter.AnyEq(d => d.Roles, MemberRole.Trainer.ToString()),
                Builders<MemberDocument>.Filter.AnyEq(d => d.Roles, MemberRole.Admin.ToString())));

        var documents = await _context.Members
            .Find(filter)
            .Sort(Builders<MemberDocument>.Sort.Ascending(d => d.LastName).Ascending(d => d.FirstName))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task AddAsync(Member member, CancellationToken ct = default)
    {
        var document = MemberDocument.FromDomain(member);
        await _context.Members.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(Member member, CancellationToken ct = default)
    {
        var document = MemberDocument.FromDomain(member);
        var expectedVersion = document.Version;
        document.Version = expectedVersion + 1;

        var filter = Builders<MemberDocument>.Filter.And(
            Builders<MemberDocument>.Filter.Eq(d => d.Id, document.Id),
            Builders<MemberDocument>.Filter.Eq(d => d.Version, expectedVersion));

        var result = await _context.Members.ReplaceOneAsync(filter, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new ConcurrencyException(nameof(Member), member.Id);
    }
}

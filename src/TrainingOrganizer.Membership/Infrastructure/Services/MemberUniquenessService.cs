using MongoDB.Driver;
using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Membership.Domain.Services;
using TrainingOrganizer.Membership.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Membership.Infrastructure.Services;

public sealed class MemberUniquenessService : IMemberUniquenessService
{
    private readonly MongoDbContext _context;

    private IMongoCollection<MemberDocument> Members => _context.Database.GetCollection<MemberDocument>("members");

    public MemberUniquenessService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsEmailUniqueAsync(
        Email email, MemberId? excludeMemberId = null, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<MemberDocument>.Filter;
        var filter = filterBuilder.Eq(d => d.Email, email.Value);

        if (excludeMemberId is not null)
        {
            filter &= filterBuilder.Ne(d => d.Id, excludeMemberId.Value);
        }

        var count = await Members.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }
}

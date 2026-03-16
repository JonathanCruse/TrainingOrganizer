using MongoDB.Driver;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Services;
using TrainingOrganizer.Infrastructure.Persistence;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Services;

public sealed class MemberUniquenessService : IMemberUniquenessService
{
    private readonly MongoDbContext _context;

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

        var count = await _context.Members.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TrainingOrganizer.Application.Membership.Services;
using TrainingOrganizer.Domain.Membership.Enums;
using TrainingOrganizer.Infrastructure.Persistence;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Seeding;

public sealed class DevDataSeeder : IHostedService
{
    private readonly MongoDbContext _context;
    private readonly IKeycloakAdminClient _keycloakAdminClient;
    private readonly ILogger<DevDataSeeder> _logger;

    public DevDataSeeder(
        MongoDbContext context,
        IKeycloakAdminClient keycloakAdminClient,
        ILogger<DevDataSeeder> logger)
    {
        _context = context;
        _keycloakAdminClient = keycloakAdminClient;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var existingCount = await _context.Members.CountDocumentsAsync(
            Builders<MemberDocument>.Filter.Empty, cancellationToken: cancellationToken);

        if (existingCount > 0)
        {
            _logger.LogInformation("Members collection already has {Count} documents, skipping seed", existingCount);
            return;
        }

        _logger.LogInformation("Seeding development data...");

        try
        {
            var now = DateTimeOffset.UtcNow;
            var adminId = Guid.NewGuid();

            var seedUsers = new[]
            {
                (Id: adminId, Email: "admin@example.com", FirstName: "Admin", LastName: "User",
                    Roles: new HashSet<string> { MemberRole.Admin.ToString(), MemberRole.Trainer.ToString(), MemberRole.Member.ToString() },
                    KeycloakRoles: new[] { "Admin", "Trainer", "Member" }),
                (Id: Guid.NewGuid(), Email: "trainer@example.com", FirstName: "Test", LastName: "Trainer",
                    Roles: new HashSet<string> { MemberRole.Trainer.ToString(), MemberRole.Member.ToString() },
                    KeycloakRoles: new[] { "Trainer", "Member" }),
                (Id: Guid.NewGuid(), Email: "member@example.com", FirstName: "Test", LastName: "Member",
                    Roles: new HashSet<string> { MemberRole.Member.ToString() },
                    KeycloakRoles: new[] { "Member" })
            };

            var documents = new List<MemberDocument>();

            foreach (var user in seedUsers)
            {
                var keycloakUserId = await _keycloakAdminClient.CreateOrGetUserAsync(
                    user.Email, user.FirstName, user.LastName, cancellationToken);

                foreach (var role in user.KeycloakRoles)
                {
                    await _keycloakAdminClient.AssignRealmRoleAsync(keycloakUserId, role, cancellationToken);
                }

                documents.Add(new MemberDocument
                {
                    Id = user.Id,
                    ExternalIdentityProvider = "keycloak",
                    ExternalIdentitySubjectId = keycloakUserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = user.Roles,
                    RegistrationStatus = RegistrationStatus.Approved.ToString(),
                    RegisteredAt = now,
                    ApprovedAt = now,
                    ApprovedBy = adminId,
                    Version = 1
                });
            }

            await _context.Members.InsertManyAsync(documents, cancellationToken: cancellationToken);
            _logger.LogInformation("Seeded {Count} members", documents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to seed development data — is Keycloak running?");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

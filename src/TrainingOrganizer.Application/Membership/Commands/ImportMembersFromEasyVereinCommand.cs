using MediatR;
using Microsoft.Extensions.Logging;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Membership.Repositories;
using TrainingOrganizer.Application.Membership.Services;
using TrainingOrganizer.Domain.Membership;
using TrainingOrganizer.Domain.Membership.Enums;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Application.Membership.Commands;

public sealed record ImportMembersFromEasyVereinCommand(string AdminGroupName) : IRequest<Result<ImportMembersResult>>;

public sealed record ImportMembersResult(
    int TotalProcessed,
    int MembersCreated,
    int MembersSkipped,
    int AdminsAssigned,
    List<string> Errors);

public sealed class ImportMembersFromEasyVereinCommandHandler
    : IRequestHandler<ImportMembersFromEasyVereinCommand, Result<ImportMembersResult>>
{
    private readonly IEasyVereinApiClient _easyVereinClient;
    private readonly IKeycloakAdminClient _keycloakClient;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportMembersFromEasyVereinCommandHandler> _logger;

    public ImportMembersFromEasyVereinCommandHandler(
        IEasyVereinApiClient easyVereinClient,
        IKeycloakAdminClient keycloakClient,
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        ILogger<ImportMembersFromEasyVereinCommandHandler> logger)
    {
        _easyVereinClient = easyVereinClient;
        _keycloakClient = keycloakClient;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ImportMembersResult>> Handle(
        ImportMembersFromEasyVereinCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var created = 0;
        var skipped = 0;
        var adminsAssigned = 0;

        // 1. Fetch all members and groups from EasyVerein
        List<EasyVereinMemberDto> easyVereinMembers;
        List<EasyVereinMemberGroupDto> easyVereinGroups;
        try
        {
            easyVereinMembers = await _easyVereinClient.GetAllMembersAsync(cancellationToken);
            easyVereinGroups = await _easyVereinClient.GetAllMemberGroupsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch data from EasyVerein API");
            return Result.Failure<ImportMembersResult>("Import.EasyVereinError", $"Failed to fetch data from EasyVerein: {ex.Message}");
        }

        // 2. Find admin group ID
        var adminGroup = easyVereinGroups.Find(g =>
            string.Equals(g.Name, request.AdminGroupName, StringComparison.OrdinalIgnoreCase));

        if (adminGroup is null)
        {
            _logger.LogWarning("Admin group '{GroupName}' not found in EasyVerein. Available groups: {Groups}",
                request.AdminGroupName,
                string.Join(", ", easyVereinGroups.Select(g => g.Name)));
        }

        _logger.LogInformation("Starting import of {Count} members from EasyVerein", easyVereinMembers.Count);

        // 3. Process each member
        foreach (var evMember in easyVereinMembers)
        {
            try
            {
                var result = await ProcessMemberAsync(evMember, adminGroup?.Id, cancellationToken);
                switch (result)
                {
                    case MemberProcessResult.Created:
                        created++;
                        break;
                    case MemberProcessResult.Skipped:
                        skipped++;
                        break;
                    case MemberProcessResult.AdminAssigned:
                        created++;
                        adminsAssigned++;
                        break;
                    case MemberProcessResult.ExistingAdminAssigned:
                        skipped++;
                        adminsAssigned++;
                        break;
                }
            }
            catch (Exception ex)
            {
                var identifier = evMember.EmailOrUserName ?? evMember.Id.ToString();
                _logger.LogWarning(ex, "Failed to import member {MemberIdentifier}", identifier);
                errors.Add($"Failed to import member {identifier}: {ex.Message}");
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var total = easyVereinMembers.Count;
        _logger.LogInformation(
            "Import complete. Processed: {Total}, Created: {Created}, Skipped: {Skipped}, Admins: {Admins}, Errors: {Errors}",
            total, created, skipped, adminsAssigned, errors.Count);

        return Result.Success(new ImportMembersResult(total, created, skipped, adminsAssigned, errors));
    }

    private async Task<MemberProcessResult> ProcessMemberAsync(
        EasyVereinMemberDto evMember, int? adminGroupId, CancellationToken ct)
    {
        var email = evMember.EmailOrUserName;
        var firstName = evMember.ContactDetails?.FirstName;
        var familyName = evMember.ContactDetails?.FamilyName;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(familyName))
        {
            _logger.LogDebug("Skipping EasyVerein member {Id} — missing email or name", evMember.Id);
            return MemberProcessResult.Skipped;
        }

        var isInAdminGroup = adminGroupId.HasValue &&
            evMember.MemberGroups?.Any(g => g.Id == adminGroupId.Value) == true;

        // Check if already imported
        var domainEmail = new Email(email);
        var existingMember = await _memberRepository.GetByEmailAsync(domainEmail, ct);

        if (existingMember is not null)
        {
            // Existing member — check if admin role needs assigning
            if (isInAdminGroup && !existingMember.HasRole(MemberRole.Admin) && existingMember.IsActive)
            {
                existingMember.AssignRole(MemberRole.Admin);
                await _memberRepository.UpdateAsync(existingMember, ct);

                var keycloakUserId = existingMember.ExternalIdentity.SubjectId;
                await _keycloakClient.AssignRealmRoleAsync(keycloakUserId, "Admin", ct);

                return MemberProcessResult.ExistingAdminAssigned;
            }

            return MemberProcessResult.Skipped;
        }

        // Create Keycloak user
        var keycloakId = await _keycloakClient.CreateOrGetUserAsync(email, firstName, familyName, ct);

        // Create domain member
        var externalIdentity = new ExternalIdentity("keycloak", keycloakId);
        var name = new PersonName(firstName, familyName);
        var member = Member.Import(externalIdentity, name, domainEmail);

        // Assign admin role if in admin group
        if (isInAdminGroup)
        {
            member.AssignRole(MemberRole.Admin);
            await _keycloakClient.AssignRealmRoleAsync(keycloakId, "Admin", ct);
        }

        await _memberRepository.AddAsync(member, ct);

        return isInAdminGroup ? MemberProcessResult.AdminAssigned : MemberProcessResult.Created;
    }

    private enum MemberProcessResult
    {
        Created,
        Skipped,
        AdminAssigned,
        ExistingAdminAssigned
    }
}

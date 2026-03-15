using TrainingOrganizer.Domain.Membership;
using TrainingOrganizer.Domain.Membership.Enums;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Tests.TestHelpers;

public static class MemberFactory
{
    private static readonly ExternalIdentity DefaultExternalIdentity =
        new("keycloak", Guid.NewGuid().ToString());

    public static Member CreatePendingMember(
        string firstName = "Jane",
        string lastName = "Doe",
        string email = "jane.doe@example.com")
    {
        return Member.Register(
            new ExternalIdentity("keycloak", Guid.NewGuid().ToString()),
            new PersonName(firstName, lastName),
            new Email(email));
    }

    public static Member CreateApprovedMember(
        string firstName = "John",
        string lastName = "Smith",
        string email = "john.smith@example.com")
    {
        var member = CreatePendingMember(firstName, lastName, email);
        var approverId = MemberId.Create();
        member.Approve(approverId);
        member.ClearDomainEvents();
        return member;
    }

    public static Member CreateTrainer(
        string firstName = "Mike",
        string lastName = "Trainer",
        string email = "mike.trainer@example.com")
    {
        var member = CreateApprovedMember(firstName, lastName, email);
        member.AssignRole(MemberRole.Trainer);
        member.ClearDomainEvents();
        return member;
    }

    public static Member CreateAdmin(
        string firstName = "Alice",
        string lastName = "Admin",
        string email = "alice.admin@example.com")
    {
        var member = CreateApprovedMember(firstName, lastName, email);
        member.AssignRole(MemberRole.Admin);
        member.ClearDomainEvents();
        return member;
    }
}

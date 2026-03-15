using FluentAssertions;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership;
using TrainingOrganizer.Domain.Membership.Enums;
using TrainingOrganizer.Domain.Membership.Events;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Tests.TestHelpers;

namespace TrainingOrganizer.Domain.Tests.Membership;

public class MemberTests
{
    private readonly ExternalIdentity _externalIdentity = new("keycloak", Guid.NewGuid().ToString());
    private readonly PersonName _name = new("Jane", "Doe");
    private readonly Email _email = new("jane.doe@example.com");

    // --- Register ---

    [Fact]
    public void Register_ValidData_CreatesMemberWithPendingStatusAndGuestRole()
    {
        var member = Member.Register(_externalIdentity, _name, _email);

        member.Id.Should().NotBeNull();
        member.RegistrationStatus.Should().Be(RegistrationStatus.Pending);
        member.Roles.Should().ContainSingle().Which.Should().Be(MemberRole.Guest);
        member.Name.Should().Be(_name);
        member.Email.Should().Be(_email);
        member.ExternalIdentity.Should().Be(_externalIdentity);
        member.RegisteredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Register_ValidData_RaisesMemberRegisteredEvent()
    {
        var member = Member.Register(_externalIdentity, _name, _email);

        member.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MemberRegisteredEvent>()
            .Which.MemberId.Should().Be(member.Id);
    }

    // --- Approve ---

    [Fact]
    public void Approve_PendingMember_TransitionsToPendingApprovedAndAddsMemberRole()
    {
        var member = MemberFactory.CreatePendingMember();
        member.ClearDomainEvents();
        var approverId = MemberId.Create();

        member.Approve(approverId);

        member.RegistrationStatus.Should().Be(RegistrationStatus.Approved);
        member.Roles.Should().Contain(MemberRole.Member);
        member.Roles.Should().NotContain(MemberRole.Guest);
        member.ApprovedAt.Should().NotBeNull();
        member.ApprovedBy.Should().Be(approverId);
    }

    [Fact]
    public void Approve_PendingMember_RaisesMemberApprovedEvent()
    {
        var member = MemberFactory.CreatePendingMember();
        member.ClearDomainEvents();
        var approverId = MemberId.Create();

        member.Approve(approverId);

        member.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MemberApprovedEvent>()
            .Which.ApprovedBy.Should().Be(approverId);
    }

    [Theory]
    [InlineData(RegistrationStatus.Approved)]
    [InlineData(RegistrationStatus.Rejected)]
    [InlineData(RegistrationStatus.Suspended)]
    public void Approve_NonPendingMember_ThrowsInvalidEntityStateException(RegistrationStatus status)
    {
        var member = CreateMemberInState(status);

        var act = () => member.Approve(MemberId.Create());

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Reject ---

    [Fact]
    public void Reject_PendingMember_TransitionsToRejected()
    {
        var member = MemberFactory.CreatePendingMember();

        member.Reject("Does not meet membership criteria.");

        member.RegistrationStatus.Should().Be(RegistrationStatus.Rejected);
    }

    [Fact]
    public void Reject_ApprovedMember_ThrowsInvalidEntityStateException()
    {
        var member = MemberFactory.CreateApprovedMember();

        var act = () => member.Reject("Some reason");

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Suspend ---

    [Fact]
    public void Suspend_ApprovedMember_TransitionsToSuspended()
    {
        var member = MemberFactory.CreateApprovedMember();

        member.Suspend("Violated terms of service.");

        member.RegistrationStatus.Should().Be(RegistrationStatus.Suspended);
    }

    [Fact]
    public void Suspend_PendingMember_ThrowsInvalidEntityStateException()
    {
        var member = MemberFactory.CreatePendingMember();

        var act = () => member.Suspend("Some reason");

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Reinstate ---

    [Fact]
    public void Reinstate_SuspendedMember_TransitionsToApproved()
    {
        var member = MemberFactory.CreateApprovedMember();
        member.Suspend("Temp suspension");

        member.Reinstate();

        member.RegistrationStatus.Should().Be(RegistrationStatus.Approved);
    }

    [Fact]
    public void Reinstate_RejectedMember_TransitionsToApproved()
    {
        var member = MemberFactory.CreatePendingMember();
        member.Reject("Rejected initially");

        member.Reinstate();

        member.RegistrationStatus.Should().Be(RegistrationStatus.Approved);
    }

    [Fact]
    public void Reinstate_PendingMember_ThrowsInvalidEntityStateException()
    {
        var member = MemberFactory.CreatePendingMember();

        var act = () => member.Reinstate();

        act.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void Reinstate_ApprovedMember_ThrowsInvalidEntityStateException()
    {
        var member = MemberFactory.CreateApprovedMember();

        var act = () => member.Reinstate();

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- AssignRole ---

    [Fact]
    public void AssignRole_TrainerOnApprovedMember_AddsRole()
    {
        var member = MemberFactory.CreateApprovedMember();

        member.AssignRole(MemberRole.Trainer);

        member.Roles.Should().Contain(MemberRole.Trainer);
    }

    [Fact]
    public void AssignRole_GuestRole_ThrowsDomainException()
    {
        var member = MemberFactory.CreateApprovedMember();

        var act = () => member.AssignRole(MemberRole.Guest);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignRole_DuplicateRole_ThrowsBusinessRuleViolationException()
    {
        var member = MemberFactory.CreateApprovedMember();
        member.AssignRole(MemberRole.Trainer);

        var act = () => member.AssignRole(MemberRole.Trainer);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void AssignRole_OnNonApprovedMember_ThrowsInvalidEntityStateException()
    {
        var member = MemberFactory.CreatePendingMember();

        var act = () => member.AssignRole(MemberRole.Trainer);

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- RemoveRole ---

    [Fact]
    public void RemoveRole_TrainerRole_RemovesRole()
    {
        var member = MemberFactory.CreateTrainer();

        member.RemoveRole(MemberRole.Trainer);

        member.Roles.Should().NotContain(MemberRole.Trainer);
    }

    [Fact]
    public void RemoveRole_BaseMemberRole_ThrowsDomainException()
    {
        var member = MemberFactory.CreateApprovedMember();

        var act = () => member.RemoveRole(MemberRole.Member);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveRole_RoleNotAssigned_ThrowsBusinessRuleViolationException()
    {
        var member = MemberFactory.CreateApprovedMember();

        var act = () => member.RemoveRole(MemberRole.Admin);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    // --- UpdateProfile ---

    [Fact]
    public void UpdateProfile_ValidData_UpdatesNameEmailPhone()
    {
        var member = MemberFactory.CreateApprovedMember();
        var newName = new PersonName("Updated", "Name");
        var newEmail = new Email("updated@example.com");
        var newPhone = new PhoneNumber("+49 123 456789");

        member.UpdateProfile(newName, newEmail, newPhone);

        member.Name.Should().Be(newName);
        member.Email.Should().Be(newEmail);
        member.Phone.Should().Be(newPhone);
    }

    // --- Computed properties ---

    [Fact]
    public void IsActive_ApprovedMember_ReturnsTrue()
    {
        var member = MemberFactory.CreateApprovedMember();

        member.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_PendingMember_ReturnsFalse()
    {
        var member = MemberFactory.CreatePendingMember();

        member.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsTrainer_MemberWithTrainerRole_ReturnsTrue()
    {
        var member = MemberFactory.CreateTrainer();

        member.IsTrainer.Should().BeTrue();
    }

    [Fact]
    public void IsTrainer_MemberWithoutTrainerRole_ReturnsFalse()
    {
        var member = MemberFactory.CreateApprovedMember();

        member.IsTrainer.Should().BeFalse();
    }

    [Fact]
    public void IsAdmin_MemberWithAdminRole_ReturnsTrue()
    {
        var member = MemberFactory.CreateAdmin();

        member.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_MemberWithoutAdminRole_ReturnsFalse()
    {
        var member = MemberFactory.CreateApprovedMember();

        member.IsAdmin.Should().BeFalse();
    }

    // --- Helper ---

    private static Member CreateMemberInState(RegistrationStatus status)
    {
        var member = MemberFactory.CreatePendingMember(email: $"{Guid.NewGuid()}@example.com");
        switch (status)
        {
            case RegistrationStatus.Approved:
                member.Approve(MemberId.Create());
                break;
            case RegistrationStatus.Rejected:
                member.Reject("Rejected");
                break;
            case RegistrationStatus.Suspended:
                member.Approve(MemberId.Create());
                member.Suspend("Suspended");
                break;
        }
        member.ClearDomainEvents();
        return member;
    }
}

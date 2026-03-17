using System.Diagnostics.CodeAnalysis;
using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.Enums;
using TrainingOrganizer.Domain.Membership.Events;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Membership;

public sealed class Member : AggregateRoot<MemberId>
{
    private PersonName _name;
    private Email _email;
    private readonly HashSet<MemberRole> _roles = [];

    public required ExternalIdentity ExternalIdentity { get; init; }
    public required PersonName Name { get => _name; init => _name = value; }
    public required Email Email { get => _email; init => _email = value; }
    public PhoneNumber? Phone { get; private set; }
    public IReadOnlySet<MemberRole> Roles => _roles;
    public RegistrationStatus RegistrationStatus { get; private set; }
    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public MemberId? ApprovedBy { get; private set; }

    [SetsRequiredMembers]
    private Member()
    {
        _name = default!;
        _email = default!;
    }

    public static Member Register(ExternalIdentity externalIdentity, PersonName name, Email email)
    {
        Guard.AgainstNull(externalIdentity, nameof(externalIdentity));
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstNull(email, nameof(email));

        var member = new Member
        {
            Id = MemberId.Create(),
            ExternalIdentity = externalIdentity,
            Name = name,
            Email = email,
            RegistrationStatus = RegistrationStatus.Pending,
            RegisteredAt = DateTimeOffset.UtcNow
        };

        member._roles.Add(MemberRole.Guest);
        member.AddDomainEvent(new MemberRegisteredEvent(member.Id, email, DateTimeOffset.UtcNow));
        return member;
    }

    public static Member Import(ExternalIdentity externalIdentity, PersonName name, Email email)
    {
        Guard.AgainstNull(externalIdentity, nameof(externalIdentity));
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstNull(email, nameof(email));

        var member = new Member
        {
            Id = MemberId.Create(),
            ExternalIdentity = externalIdentity,
            Name = name,
            Email = email,
            RegistrationStatus = RegistrationStatus.Approved,
            RegisteredAt = DateTimeOffset.UtcNow,
            ApprovedAt = DateTimeOffset.UtcNow
        };

        member._roles.Add(MemberRole.Member);
        member.AddDomainEvent(new MemberImportedEvent(member.Id, email, externalIdentity.Provider, DateTimeOffset.UtcNow));
        return member;
    }

    public void Approve(MemberId approvedBy)
    {
        Guard.AgainstNull(approvedBy, nameof(approvedBy));

        if (RegistrationStatus != RegistrationStatus.Pending)
            throw new InvalidEntityStateException(nameof(Member), RegistrationStatus.ToString(), "approve");

        RegistrationStatus = RegistrationStatus.Approved;
        _roles.Remove(MemberRole.Guest);
        _roles.Add(MemberRole.Member);
        ApprovedAt = DateTimeOffset.UtcNow;
        ApprovedBy = approvedBy;

        AddDomainEvent(new MemberApprovedEvent(Id, approvedBy, DateTimeOffset.UtcNow));
    }

    public void Reject(string reason)
    {
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));

        if (RegistrationStatus != RegistrationStatus.Pending)
            throw new InvalidEntityStateException(nameof(Member), RegistrationStatus.ToString(), "reject");

        RegistrationStatus = RegistrationStatus.Rejected;

        AddDomainEvent(new MemberRejectedEvent(Id, reason, DateTimeOffset.UtcNow));
    }

    public void Suspend(string reason)
    {
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));

        if (RegistrationStatus != RegistrationStatus.Approved)
            throw new InvalidEntityStateException(nameof(Member), RegistrationStatus.ToString(), "suspend");

        RegistrationStatus = RegistrationStatus.Suspended;

        AddDomainEvent(new MemberSuspendedEvent(Id, reason, DateTimeOffset.UtcNow));
    }

    public void Reinstate()
    {
        if (RegistrationStatus is not (RegistrationStatus.Suspended or RegistrationStatus.Rejected))
            throw new InvalidEntityStateException(nameof(Member), RegistrationStatus.ToString(), "reinstate");

        RegistrationStatus = RegistrationStatus.Approved;
    }

    public void AssignRole(MemberRole role)
    {
        if (RegistrationStatus != RegistrationStatus.Approved)
            throw new InvalidEntityStateException(nameof(Member), RegistrationStatus.ToString(), "assign role");

        Guard.AgainstCondition(role == MemberRole.Guest, "Cannot assign Guest role explicitly.");

        if (!_roles.Add(role))
            throw new BusinessRuleViolationException("DuplicateRole", $"Member already has the '{role}' role.");

        AddDomainEvent(new RoleAssignedEvent(Id, role, DateTimeOffset.UtcNow));
    }

    public void RemoveRole(MemberRole role)
    {
        if (RegistrationStatus != RegistrationStatus.Approved)
            throw new InvalidEntityStateException(nameof(Member), RegistrationStatus.ToString(), "remove role");

        Guard.AgainstCondition(role == MemberRole.Member, "Cannot remove the base Member role.");

        if (!_roles.Remove(role))
            throw new BusinessRuleViolationException("RoleNotAssigned", $"Member does not have the '{role}' role.");

        AddDomainEvent(new RoleRemovedEvent(Id, role, DateTimeOffset.UtcNow));
    }

    public void UpdateProfile(PersonName name, Email email, PhoneNumber? phone = null)
    {
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstNull(email, nameof(email));

        _name = name;
        _email = email;
        Phone = phone;
    }

    public bool HasRole(MemberRole role) => _roles.Contains(role);
    public bool IsActive => RegistrationStatus == RegistrationStatus.Approved;
    public bool IsTrainer => _roles.Contains(MemberRole.Trainer);
    public bool IsAdmin => _roles.Contains(MemberRole.Admin);
}

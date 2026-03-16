using MongoDB.Bson.Serialization.Attributes;
using TrainingOrganizer.Domain.Membership;
using TrainingOrganizer.Domain.Membership.Enums;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Infrastructure.Persistence.Documents;

public sealed class MemberDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public string ExternalIdentityProvider { get; set; } = string.Empty;
    public string ExternalIdentitySubjectId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public HashSet<string> Roles { get; set; } = [];
    public string RegistrationStatus { get; set; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public int Version { get; set; }

    public static MemberDocument FromDomain(Member member)
    {
        return new MemberDocument
        {
            Id = member.Id.Value,
            ExternalIdentityProvider = member.ExternalIdentity.Provider,
            ExternalIdentitySubjectId = member.ExternalIdentity.SubjectId,
            FirstName = member.Name.FirstName,
            LastName = member.Name.LastName,
            Email = member.Email.Value,
            Phone = member.Phone?.Value,
            Roles = member.Roles.Select(r => r.ToString()).ToHashSet(),
            RegistrationStatus = member.RegistrationStatus.ToString(),
            RegisteredAt = member.RegisteredAt,
            ApprovedAt = member.ApprovedAt,
            ApprovedBy = member.ApprovedBy?.Value,
            Version = member.Version
        };
    }

    public Member ToDomain()
    {
        var member = DomainObjectMapper.CreateInstance<Member>();

        DomainObjectMapper.SetProperty(member, "Id", new MemberId(Id));
        DomainObjectMapper.SetProperty(member, "ExternalIdentity",
            new ExternalIdentity(ExternalIdentityProvider, ExternalIdentitySubjectId));
        DomainObjectMapper.SetProperty(member, "Name", new PersonName(FirstName, LastName));
        DomainObjectMapper.SetProperty(member, "Email", new Email(Email));
        DomainObjectMapper.SetProperty(member, "Phone",
            Phone is not null ? new PhoneNumber(Phone) : null);

        var roles = Roles.Select(r => Enum.Parse<MemberRole>(r));
        DomainObjectMapper.AddToHashSet(member, "_roles", roles);

        DomainObjectMapper.SetProperty(member, "RegistrationStatus",
            Enum.Parse<RegistrationStatus>(RegistrationStatus));
        DomainObjectMapper.SetProperty(member, "RegisteredAt", RegisteredAt);
        DomainObjectMapper.SetProperty(member, "ApprovedAt", ApprovedAt);
        DomainObjectMapper.SetProperty(member, "ApprovedBy",
            ApprovedBy.HasValue ? new MemberId(ApprovedBy.Value) : null);
        DomainObjectMapper.SetProperty(member, "Version", Version);

        return member;
    }
}

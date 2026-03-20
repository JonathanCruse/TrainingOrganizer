using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Membership.Domain.ValueObjects;

public sealed record MemberId : StronglyTypedId
{
    public MemberId(Guid value) : base(value) { }

    public static MemberId Create() => new(Guid.NewGuid());
}

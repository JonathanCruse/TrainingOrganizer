using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Membership.ValueObjects;

public sealed record MemberId : StronglyTypedId
{
    public MemberId(Guid value) : base(value) { }

    public static MemberId Create() => new(Guid.NewGuid());
}

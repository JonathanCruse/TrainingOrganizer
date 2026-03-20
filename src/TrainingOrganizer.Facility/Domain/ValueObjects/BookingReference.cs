using TrainingOrganizer.SharedKernel.Domain;

namespace TrainingOrganizer.Facility.Domain.ValueObjects;

public enum BookingReferenceType
{
    Training = 0,
    TrainingSession = 1,
    Manual = 2
}

public sealed record BookingReference : ValueObject
{
    public BookingReferenceType ReferenceType { get; }
    public Guid ReferenceId { get; }

    public BookingReference(BookingReferenceType referenceType, Guid referenceId)
    {
        Guard.AgainstCondition(referenceId == Guid.Empty, "BookingReference ReferenceId cannot be empty.");
        ReferenceType = referenceType;
        ReferenceId = referenceId;
    }
}

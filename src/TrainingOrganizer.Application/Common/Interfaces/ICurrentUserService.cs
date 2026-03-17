using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Application.Common.Interfaces;

public interface ICurrentUserService
{
    MemberId? MemberId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsTrainer { get; }
    bool IsGuest { get; }
}

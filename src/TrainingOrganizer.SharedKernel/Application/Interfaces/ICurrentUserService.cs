namespace TrainingOrganizer.SharedKernel.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? MemberId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsTrainer { get; }
    bool IsGuest { get; }
}

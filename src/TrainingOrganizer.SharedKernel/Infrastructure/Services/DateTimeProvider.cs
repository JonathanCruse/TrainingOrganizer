using TrainingOrganizer.SharedKernel.Application.Interfaces;

namespace TrainingOrganizer.SharedKernel.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}

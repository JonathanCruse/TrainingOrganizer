using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;
using DomainTraining = TrainingOrganizer.Training.Domain.Training;
using DomainRecurringTraining = TrainingOrganizer.Training.Domain.RecurringTraining;

namespace TrainingOrganizer.Training.Tests.TestHelpers;

public static class TrainingFactory
{
    public static TimeSlot CreateTimeSlot(
        DateTimeOffset? start = null,
        TimeSpan? duration = null)
    {
        var s = start ?? DateTimeOffset.UtcNow.AddDays(7);
        var d = duration ?? TimeSpan.FromHours(2);
        return new TimeSlot(s, s.Add(d));
    }

    public static Capacity CreateCapacity(int min = 2, int max = 10)
    {
        return new Capacity(min, max);
    }

    public static DomainTraining CreateDraftTraining(
        TrainingTitle? title = null,
        TrainingDescription? description = null,
        TimeSlot? timeSlot = null,
        Capacity? capacity = null,
        Visibility visibility = Visibility.Public,
        IReadOnlyList<MemberId>? trainerIds = null,
        MemberId? createdBy = null)
    {
        var training = DomainTraining.Create(
            title ?? new TrainingTitle("Advanced C# Workshop"),
            description ?? new TrainingDescription("A deep dive into advanced C# features."),
            timeSlot ?? CreateTimeSlot(),
            capacity ?? CreateCapacity(),
            visibility,
            trainerIds ?? [MemberId.Create()],
            createdBy ?? MemberId.Create());

        training.ClearDomainEvents();
        return training;
    }

    public static DomainTraining CreatePublishedTraining(
        TrainingTitle? title = null,
        TrainingDescription? description = null,
        TimeSlot? timeSlot = null,
        Capacity? capacity = null,
        Visibility visibility = Visibility.Public,
        IReadOnlyList<MemberId>? trainerIds = null,
        MemberId? createdBy = null)
    {
        var training = CreateDraftTraining(title, description, timeSlot, capacity, visibility, trainerIds, createdBy);
        training.Publish();
        training.ClearDomainEvents();
        return training;
    }

    public static TrainingTemplate CreateTemplate(
        TrainingTitle? title = null,
        TrainingDescription? description = null,
        Capacity? capacity = null,
        Visibility visibility = Visibility.Public,
        IReadOnlyList<MemberId>? trainerIds = null,
        IReadOnlyList<RoomRequirement>? roomRequirements = null)
    {
        return new TrainingTemplate(
            title ?? new TrainingTitle("Weekly Yoga Class"),
            description ?? new TrainingDescription("Relaxing yoga session for all levels."),
            capacity ?? CreateCapacity(),
            visibility,
            trainerIds ?? [MemberId.Create()],
            roomRequirements ?? []);
    }

    public static RecurrenceRule CreateWeeklyRule(
        DayOfWeek dayOfWeek = DayOfWeek.Monday,
        TimeOnly? timeOfDay = null,
        TimeSpan? duration = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        return new RecurrenceRule(
            RecurrencePattern.Weekly,
            dayOfWeek,
            timeOfDay ?? new TimeOnly(10, 0),
            duration ?? TimeSpan.FromHours(1),
            startDate ?? new DateOnly(2026, 1, 5),
            endDate);
    }
}

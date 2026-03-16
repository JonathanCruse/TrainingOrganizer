using FluentAssertions;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Tests.TestHelpers;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.Events;
using TrainingOrganizer.Domain.Training.ValueObjects;
using RecurringTraining = TrainingOrganizer.Domain.Training.RecurringTraining;

namespace TrainingOrganizer.Domain.Tests.Training;

public class RecurringTrainingTests
{
    // --- Create ---

    [Fact]
    public void Create_ValidData_ReturnsActiveRecurringTraining()
    {
        var template = TrainingFactory.CreateTemplate();
        var rule = TrainingFactory.CreateWeeklyRule();
        var createdBy = MemberId.Create();

        var recurring = RecurringTraining.Create(template, rule, createdBy);

        recurring.Id.Should().NotBeNull();
        recurring.Status.Should().Be(RecurringTrainingStatus.Active);
        recurring.Template.Should().Be(template);
        recurring.RecurrenceRule.Should().Be(rule);
        recurring.CreatedBy.Should().Be(createdBy);
        recurring.LastGeneratedUntil.Should().BeNull();
    }

    [Fact]
    public void Create_ValidData_RaisesRecurringTrainingCreatedEvent()
    {
        var template = TrainingFactory.CreateTemplate();
        var rule = TrainingFactory.CreateWeeklyRule();

        var recurring = RecurringTraining.Create(template, rule, MemberId.Create());

        recurring.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RecurringTrainingCreatedEvent>()
            .Which.RecurringTrainingId.Should().Be(recurring.Id);
    }

    // --- UpdateTemplate ---

    [Fact]
    public void UpdateTemplate_ActiveRecurring_UpdatesTemplate()
    {
        var recurring = CreateActiveRecurring();
        var newTemplate = TrainingFactory.CreateTemplate(
            title: new TrainingTitle("Updated Yoga"),
            capacity: new Capacity(5, 25));

        recurring.UpdateTemplate(newTemplate);

        recurring.Template.Should().Be(newTemplate);
    }

    [Fact]
    public void UpdateTemplate_EndedRecurring_ThrowsInvalidEntityStateException()
    {
        var recurring = CreateActiveRecurring();
        recurring.End();
        recurring.ClearDomainEvents();

        var act = () => recurring.UpdateTemplate(TrainingFactory.CreateTemplate());

        act.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void UpdateTemplate_PausedRecurring_UpdatesTemplate()
    {
        var recurring = CreateActiveRecurring();
        recurring.Pause();
        recurring.ClearDomainEvents();
        var newTemplate = TrainingFactory.CreateTemplate(title: new TrainingTitle("Paused Update"));

        recurring.UpdateTemplate(newTemplate);

        recurring.Template.Should().Be(newTemplate);
    }

    // --- Pause ---

    [Fact]
    public void Pause_ActiveRecurring_TransitionsToPaused()
    {
        var recurring = CreateActiveRecurring();

        recurring.Pause();

        recurring.Status.Should().Be(RecurringTrainingStatus.Paused);
    }

    [Fact]
    public void Pause_PausedRecurring_ThrowsInvalidEntityStateException()
    {
        var recurring = CreateActiveRecurring();
        recurring.Pause();

        var act = () => recurring.Pause();

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Resume ---

    [Fact]
    public void Resume_PausedRecurring_TransitionsToActive()
    {
        var recurring = CreateActiveRecurring();
        recurring.Pause();
        recurring.ClearDomainEvents();

        recurring.Resume();

        recurring.Status.Should().Be(RecurringTrainingStatus.Active);
    }

    [Fact]
    public void Resume_ActiveRecurring_ThrowsInvalidEntityStateException()
    {
        var recurring = CreateActiveRecurring();

        var act = () => recurring.Resume();

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- End ---

    [Fact]
    public void End_ActiveRecurring_TransitionsToEnded()
    {
        var recurring = CreateActiveRecurring();

        recurring.End();

        recurring.Status.Should().Be(RecurringTrainingStatus.Ended);
    }

    [Fact]
    public void End_PausedRecurring_TransitionsToEnded()
    {
        var recurring = CreateActiveRecurring();
        recurring.Pause();
        recurring.ClearDomainEvents();

        recurring.End();

        recurring.Status.Should().Be(RecurringTrainingStatus.Ended);
    }

    [Fact]
    public void End_EndedRecurring_ThrowsInvalidEntityStateException()
    {
        var recurring = CreateActiveRecurring();
        recurring.End();

        var act = () => recurring.End();

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- GenerateSessionsUntil ---

    [Fact]
    public void GenerateSessionsUntil_ActiveRecurring_RaisesSessionsRequestedEvent()
    {
        // Start on a Monday: 2026-01-05
        var rule = TrainingFactory.CreateWeeklyRule(
            dayOfWeek: DayOfWeek.Monday,
            startDate: new DateOnly(2026, 1, 5));
        var template = TrainingFactory.CreateTemplate();
        var recurring = RecurringTraining.Create(template, rule, MemberId.Create());
        recurring.ClearDomainEvents();

        // Generate 4 weeks of sessions
        recurring.GenerateSessionsUntil(new DateOnly(2026, 1, 26));

        recurring.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SessionsRequestedEvent>();

        var evt = (SessionsRequestedEvent)recurring.DomainEvents.Single();
        evt.OccurrenceDates.Should().HaveCount(4);
        evt.OccurrenceDates.Should().Contain(new DateOnly(2026, 1, 5));
        evt.OccurrenceDates.Should().Contain(new DateOnly(2026, 1, 12));
        evt.OccurrenceDates.Should().Contain(new DateOnly(2026, 1, 19));
        evt.OccurrenceDates.Should().Contain(new DateOnly(2026, 1, 26));
    }

    [Fact]
    public void GenerateSessionsUntil_ActiveRecurring_UpdatesLastGeneratedUntil()
    {
        var rule = TrainingFactory.CreateWeeklyRule(
            dayOfWeek: DayOfWeek.Monday,
            startDate: new DateOnly(2026, 1, 5));
        var recurring = RecurringTraining.Create(
            TrainingFactory.CreateTemplate(), rule, MemberId.Create());
        recurring.ClearDomainEvents();
        var until = new DateOnly(2026, 1, 26);

        recurring.GenerateSessionsUntil(until);

        recurring.LastGeneratedUntil.Should().Be(until);
    }

    [Fact]
    public void GenerateSessionsUntil_PausedRecurring_ThrowsInvalidEntityStateException()
    {
        var recurring = CreateActiveRecurring();
        recurring.Pause();

        var act = () => recurring.GenerateSessionsUntil(new DateOnly(2026, 3, 1));

        act.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void GenerateSessionsUntil_NoOccurrences_DoesNotRaiseEvent()
    {
        // Rule starts Monday 2026-01-05, generate until 2026-01-04 (before start)
        var rule = TrainingFactory.CreateWeeklyRule(
            dayOfWeek: DayOfWeek.Monday,
            startDate: new DateOnly(2026, 1, 5));
        var recurring = RecurringTraining.Create(
            TrainingFactory.CreateTemplate(), rule, MemberId.Create());
        recurring.ClearDomainEvents();

        recurring.GenerateSessionsUntil(new DateOnly(2026, 1, 4));

        recurring.DomainEvents.Should().BeEmpty();
        recurring.LastGeneratedUntil.Should().BeNull();
    }

    // --- Helper ---

    private static RecurringTraining CreateActiveRecurring()
    {
        var recurring = RecurringTraining.Create(
            TrainingFactory.CreateTemplate(),
            TrainingFactory.CreateWeeklyRule(),
            MemberId.Create());
        recurring.ClearDomainEvents();
        return recurring;
    }
}

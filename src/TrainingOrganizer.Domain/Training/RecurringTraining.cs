using System.Diagnostics.CodeAnalysis;
using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.Events;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Training;

public sealed class RecurringTraining : AggregateRoot<RecurringTrainingId>
{
    private TrainingTemplate _template;

    public required TrainingTemplate Template { get => _template; init => _template = value; }
    public required RecurrenceRule RecurrenceRule { get; init; }
    public RecurringTrainingStatus Status { get; private set; }
    public DateOnly? LastGeneratedUntil { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public required MemberId CreatedBy { get; init; }

    [SetsRequiredMembers]
    private RecurringTraining()
    {
        _template = default!;
    }

    public static RecurringTraining Create(
        TrainingTemplate template,
        RecurrenceRule recurrenceRule,
        MemberId createdBy)
    {
        Guard.AgainstNull(template, nameof(template));
        Guard.AgainstNull(recurrenceRule, nameof(recurrenceRule));
        Guard.AgainstNull(createdBy, nameof(createdBy));

        var recurring = new RecurringTraining
        {
            Id = RecurringTrainingId.Create(),
            Template = template,
            RecurrenceRule = recurrenceRule,
            Status = RecurringTrainingStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        recurring.AddDomainEvent(new RecurringTrainingCreatedEvent(recurring.Id, DateTimeOffset.UtcNow));

        return recurring;
    }

    public void UpdateTemplate(TrainingTemplate template)
    {
        Guard.AgainstNull(template, nameof(template));

        if (Status == RecurringTrainingStatus.Ended)
            throw new InvalidEntityStateException(nameof(RecurringTraining), Status.ToString(), "update template");

        _template = template;

        AddDomainEvent(new RecurringTrainingTemplateUpdatedEvent(Id, template, DateTimeOffset.UtcNow));
    }

    public void Pause()
    {
        if (Status != RecurringTrainingStatus.Active)
            throw new InvalidEntityStateException(nameof(RecurringTraining), Status.ToString(), "pause");

        Status = RecurringTrainingStatus.Paused;

        AddDomainEvent(new RecurringTrainingPausedEvent(Id, DateTimeOffset.UtcNow));
    }

    public void Resume()
    {
        if (Status != RecurringTrainingStatus.Paused)
            throw new InvalidEntityStateException(nameof(RecurringTraining), Status.ToString(), "resume");

        Status = RecurringTrainingStatus.Active;
    }

    public void End()
    {
        if (Status == RecurringTrainingStatus.Ended)
            throw new InvalidEntityStateException(nameof(RecurringTraining), Status.ToString(), "end");

        Status = RecurringTrainingStatus.Ended;

        AddDomainEvent(new RecurringTrainingEndedEvent(Id, DateTimeOffset.UtcNow));
    }

    public void GenerateSessionsUntil(DateOnly until)
    {
        if (Status != RecurringTrainingStatus.Active)
            throw new InvalidEntityStateException(nameof(RecurringTraining), Status.ToString(), "generate sessions");

        var from = LastGeneratedUntil?.AddDays(1)
                   ?? RecurrenceRule.StartDate;

        var occurrences = RecurrenceRule.GetOccurrences(from, until);

        if (occurrences.Count == 0)
            return;

        LastGeneratedUntil = until;

        AddDomainEvent(new SessionsRequestedEvent(
            Id,
            Template,
            occurrences,
            RecurrenceRule,
            DateTimeOffset.UtcNow));
    }
}

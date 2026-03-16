using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Schedule.DTOs;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Application.Schedule.Queries;

public sealed record GetTrainerScheduleQuery(
    Guid TrainerId,
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<Result<IReadOnlyList<ScheduleEntryDto>>>;

public sealed class GetTrainerScheduleQueryHandler : IRequestHandler<GetTrainerScheduleQuery, Result<IReadOnlyList<ScheduleEntryDto>>>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly ITrainingSessionRepository _sessionRepository;

    public GetTrainerScheduleQueryHandler(
        ITrainingRepository trainingRepository,
        ITrainingSessionRepository sessionRepository)
    {
        _trainingRepository = trainingRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<IReadOnlyList<ScheduleEntryDto>>> Handle(GetTrainerScheduleQuery request, CancellationToken cancellationToken)
    {
        var trainerId = new MemberId(request.TrainerId);

        var trainings = await _trainingRepository.GetByTrainerAsync(trainerId, request.From, request.To, cancellationToken);
        var sessions = await _sessionRepository.GetByMemberParticipationAsync(trainerId, cancellationToken);

        var entries = new List<ScheduleEntryDto>();

        foreach (var training in trainings)
        {
            entries.Add(new ScheduleEntryDto(
                training.Id.Value,
                "Training",
                training.Title.Value,
                training.TimeSlot.Start,
                training.TimeSlot.End,
                null,
                null));
        }

        foreach (var session in sessions.Where(s => s.TimeSlot.Start >= request.From && s.TimeSlot.Start <= request.To))
        {
            entries.Add(new ScheduleEntryDto(
                session.Id.Value,
                "Session",
                session.EffectiveTitle.Value,
                session.TimeSlot.Start,
                session.TimeSlot.End,
                null,
                null));
        }

        var sorted = entries.OrderBy(e => e.Start).ToList();

        return Result.Success<IReadOnlyList<ScheduleEntryDto>>(sorted);
    }
}

public sealed class GetTrainerScheduleQueryValidator : AbstractValidator<GetTrainerScheduleQuery>
{
    public GetTrainerScheduleQueryValidator()
    {
        RuleFor(x => x.TrainerId).NotEmpty();
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty().GreaterThan(x => x.From)
            .WithMessage("To must be after From.");
    }
}

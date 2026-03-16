using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Schedule.DTOs;
using TrainingOrganizer.Application.Training.Repositories;

namespace TrainingOrganizer.Application.Schedule.Queries;

public sealed record GetPersonalScheduleQuery(
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<Result<IReadOnlyList<ScheduleEntryDto>>>;

public sealed class GetPersonalScheduleQueryHandler : IRequestHandler<GetPersonalScheduleQuery, Result<IReadOnlyList<ScheduleEntryDto>>>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPersonalScheduleQueryHandler(
        ITrainingRepository trainingRepository,
        ITrainingSessionRepository sessionRepository,
        ICurrentUserService currentUserService)
    {
        _trainingRepository = trainingRepository;
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<ScheduleEntryDto>>> Handle(GetPersonalScheduleQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.MemberId
            ?? throw new ForbiddenException("You must be authenticated to view your schedule.");

        var trainings = await _trainingRepository.GetByMemberParticipationAsync(currentUserId, cancellationToken);
        var trainerTrainings = await _trainingRepository.GetByTrainerAsync(currentUserId, request.From, request.To, cancellationToken);
        var sessions = await _sessionRepository.GetByMemberParticipationAsync(currentUserId, cancellationToken);

        var entries = new List<ScheduleEntryDto>();

        foreach (var training in trainings.Where(t => t.TimeSlot.Start >= request.From && t.TimeSlot.Start <= request.To))
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

        foreach (var training in trainerTrainings.Where(t => !entries.Any(e => e.Id == t.Id.Value)))
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

public sealed class GetPersonalScheduleQueryValidator : AbstractValidator<GetPersonalScheduleQuery>
{
    public GetPersonalScheduleQueryValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty().GreaterThan(x => x.From)
            .WithMessage("To must be after From.");
    }
}

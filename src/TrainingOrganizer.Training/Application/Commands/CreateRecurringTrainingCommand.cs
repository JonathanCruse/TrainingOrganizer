using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.DTOs;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Facility.Domain.ValueObjects;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Commands;

public sealed record CreateRecurringTrainingCommand(
    string Title,
    string? Description,
    int MinCapacity,
    int MaxCapacity,
    Visibility Visibility,
    List<Guid> TrainerIds,
    List<RoomRequirementDto> RoomRequirements,
    RecurrencePattern Pattern,
    DayOfWeek DayOfWeek,
    string TimeOfDay,
    string Duration,
    string StartDate,
    string? EndDate) : IRequest<Result<Guid>>;

public sealed class CreateRecurringTrainingCommandHandler : IRequestHandler<CreateRecurringTrainingCommand, Result<Guid>>
{
    private readonly IRecurringTrainingRepository _recurringTrainingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRecurringTrainingCommandHandler(
        IRecurringTrainingRepository recurringTrainingRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _recurringTrainingRepository = recurringTrainingRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateRecurringTrainingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = new MemberId(_currentUserService.MemberId
                ?? throw new ForbiddenException("You must be authenticated to create a recurring training."));

            var title = new TrainingTitle(request.Title);
            var description = new TrainingDescription(request.Description ?? string.Empty);
            var capacity = new Capacity(request.MinCapacity, request.MaxCapacity);
            var trainerIds = request.TrainerIds.Select(id => new MemberId(id)).ToList();
            var roomRequirements = request.RoomRequirements
                .Select(r => new RoomRequirement(new RoomId(r.RoomId), new LocationId(r.LocationId)))
                .ToList();

            var template = new TrainingTemplate(
                title, description, capacity, request.Visibility, trainerIds, roomRequirements);

            var timeOfDay = TimeOnly.Parse(request.TimeOfDay);
            var duration = TimeSpan.Parse(request.Duration);
            var startDate = DateOnly.Parse(request.StartDate);
            var endDate = request.EndDate is not null ? DateOnly.Parse(request.EndDate) : (DateOnly?)null;

            var recurrenceRule = new RecurrenceRule(
                request.Pattern, request.DayOfWeek, timeOfDay, duration, startDate, endDate);

            var recurringTraining = RecurringTraining.Create(template, recurrenceRule, currentUserId);

            await _recurringTrainingRepository.AddAsync(recurringTraining, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(recurringTraining.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>("RecurringTraining.DomainError", ex.Message);
        }
    }
}

public sealed class CreateRecurringTrainingCommandValidator : AbstractValidator<CreateRecurringTrainingCommand>
{
    public CreateRecurringTrainingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000).When(x => x.Description is not null);
        RuleFor(x => x.MinCapacity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxCapacity).GreaterThan(0)
            .GreaterThanOrEqualTo(x => x.MinCapacity)
            .WithMessage("MaxCapacity must be greater than or equal to MinCapacity.");
        RuleFor(x => x.Visibility).IsInEnum();
        RuleFor(x => x.TrainerIds).NotEmpty()
            .WithMessage("A recurring training must have at least one trainer.");
        RuleForEach(x => x.TrainerIds).NotEmpty();
        RuleFor(x => x.Pattern).IsInEnum();
        RuleFor(x => x.DayOfWeek).IsInEnum();
        RuleFor(x => x.TimeOfDay).NotEmpty();
        RuleFor(x => x.Duration).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
    }
}

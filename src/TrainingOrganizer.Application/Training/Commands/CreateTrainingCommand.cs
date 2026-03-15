using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Commands;

public sealed record CreateTrainingCommand(
    string Title,
    string? Description,
    DateTimeOffset Start,
    DateTimeOffset End,
    int MinCapacity,
    int MaxCapacity,
    Visibility Visibility,
    List<Guid> TrainerIds) : IRequest<Result<Guid>>;

public sealed class CreateTrainingCommandHandler : IRequestHandler<CreateTrainingCommand, Result<Guid>>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainingCommandHandler(
        ITrainingRepository trainingRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateTrainingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.MemberId
                ?? throw new ForbiddenException("You must be authenticated to create a training.");

            var title = new TrainingTitle(request.Title);
            var description = new TrainingDescription(request.Description ?? string.Empty);
            var timeSlot = new TimeSlot(request.Start, request.End);
            var capacity = new Capacity(request.MinCapacity, request.MaxCapacity);
            var trainerIds = request.TrainerIds.Select(id => new MemberId(id)).ToList();

            var training = Domain.Training.Training.Create(
                title, description, timeSlot, capacity, request.Visibility, trainerIds, currentUserId);

            await _trainingRepository.AddAsync(training, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(training.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>("Training.DomainError", ex.Message);
        }
    }
}

public sealed class CreateTrainingCommandValidator : AbstractValidator<CreateTrainingCommand>
{
    public CreateTrainingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000).When(x => x.Description is not null);
        RuleFor(x => x.Start).NotEmpty();
        RuleFor(x => x.End).NotEmpty().GreaterThan(x => x.Start)
            .WithMessage("End must be after Start.");
        RuleFor(x => x.MinCapacity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxCapacity).GreaterThan(0)
            .GreaterThanOrEqualTo(x => x.MinCapacity)
            .WithMessage("MaxCapacity must be greater than or equal to MinCapacity.");
        RuleFor(x => x.Visibility).IsInEnum();
        RuleFor(x => x.TrainerIds).NotEmpty()
            .WithMessage("A training must have at least one trainer.");
        RuleForEach(x => x.TrainerIds).NotEmpty();
    }
}

using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Commands;

public sealed record ApplySessionOverridesCommand(
    Guid SessionId,
    string? Title,
    string? Description,
    int? MinCapacity,
    int? MaxCapacity,
    Visibility? Visibility) : IRequest<Result>;

public sealed class ApplySessionOverridesCommandHandler : IRequestHandler<ApplySessionOverridesCommand, Result>
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplySessionOverridesCommandHandler(
        ITrainingSessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ApplySessionOverridesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sessionId = new TrainingSessionId(request.SessionId);
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
                ?? throw new NotFoundException(nameof(TrainingSession), request.SessionId);

            var overrides = new SessionOverrides
            {
                Title = request.Title is not null ? new TrainingTitle(request.Title) : null,
                Description = request.Description is not null ? new TrainingDescription(request.Description) : null,
                Capacity = request.MinCapacity.HasValue && request.MaxCapacity.HasValue
                    ? new Capacity(request.MinCapacity.Value, request.MaxCapacity.Value)
                    : null,
                Visibility = request.Visibility
            };

            session.ApplyOverrides(overrides);

            await _sessionRepository.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure("Session.DomainError", ex.Message);
        }
    }
}

public sealed class ApplySessionOverridesCommandValidator : AbstractValidator<ApplySessionOverridesCommand>
{
    public ApplySessionOverridesCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
        RuleFor(x => x.Description).MaximumLength(4000).When(x => x.Description is not null);
        RuleFor(x => x.MaxCapacity).GreaterThan(0)
            .When(x => x.MaxCapacity.HasValue);
        RuleFor(x => x.MaxCapacity).GreaterThanOrEqualTo(x => x.MinCapacity)
            .When(x => x.MinCapacity.HasValue && x.MaxCapacity.HasValue)
            .WithMessage("MaxCapacity must be greater than or equal to MinCapacity.");
        RuleFor(x => x.Visibility).IsInEnum()
            .When(x => x.Visibility.HasValue);
    }
}

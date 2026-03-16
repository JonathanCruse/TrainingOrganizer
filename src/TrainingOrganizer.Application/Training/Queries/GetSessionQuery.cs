using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.DTOs;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Queries;

public sealed record GetSessionQuery(Guid SessionId) : IRequest<Result<TrainingSessionDto>>;

public sealed class GetSessionQueryHandler : IRequestHandler<GetSessionQuery, Result<TrainingSessionDto>>
{
    private readonly ITrainingSessionRepository _sessionRepository;

    public GetSessionQueryHandler(ITrainingSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<TrainingSessionDto>> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var sessionId = new TrainingSessionId(request.SessionId);
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new NotFoundException(nameof(TrainingSession), request.SessionId);

        return Result.Success(TrainingSessionDto.FromDomain(session));
    }
}

public sealed class GetSessionQueryValidator : AbstractValidator<GetSessionQuery>
{
    public GetSessionQueryValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}

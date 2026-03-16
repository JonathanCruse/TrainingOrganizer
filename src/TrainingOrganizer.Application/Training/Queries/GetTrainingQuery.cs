using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.DTOs;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Queries;

public sealed record GetTrainingQuery(Guid TrainingId) : IRequest<Result<TrainingDto>>;

public sealed class GetTrainingQueryHandler : IRequestHandler<GetTrainingQuery, Result<TrainingDto>>
{
    private readonly ITrainingRepository _trainingRepository;

    public GetTrainingQueryHandler(ITrainingRepository trainingRepository)
    {
        _trainingRepository = trainingRepository;
    }

    public async Task<Result<TrainingDto>> Handle(GetTrainingQuery request, CancellationToken cancellationToken)
    {
        var trainingId = new TrainingId(request.TrainingId);
        var training = await _trainingRepository.GetByIdAsync(trainingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Training.Training), request.TrainingId);

        return Result.Success(TrainingDto.FromDomain(training));
    }
}

public sealed class GetTrainingQueryValidator : AbstractValidator<GetTrainingQuery>
{
    public GetTrainingQueryValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
    }
}

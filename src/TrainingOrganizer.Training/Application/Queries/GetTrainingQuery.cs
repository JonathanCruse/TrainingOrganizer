using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.DTOs;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Application.Queries;

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
            ?? throw new NotFoundException(nameof(Domain.Training), request.TrainingId);

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

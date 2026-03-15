using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Training.Commands;

public sealed record AttendanceEntry(Guid MemberId, bool Attended);

public sealed record RecordTrainingAttendanceCommand(
    Guid TrainingId,
    List<AttendanceEntry> Entries) : IRequest<Result>;

public sealed class RecordTrainingAttendanceCommandHandler : IRequestHandler<RecordTrainingAttendanceCommand, Result>
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordTrainingAttendanceCommandHandler(
        ITrainingRepository trainingRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RecordTrainingAttendanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var trainingId = new TrainingId(request.TrainingId);
            var training = await _trainingRepository.GetByIdAsync(trainingId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Training.Training), request.TrainingId);

            foreach (var entry in request.Entries)
            {
                var memberId = new MemberId(entry.MemberId);
                training.RecordAttendance(memberId, entry.Attended);
            }

            await _trainingRepository.UpdateAsync(training, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure("Training.DomainError", ex.Message);
        }
    }
}

public sealed class RecordTrainingAttendanceCommandValidator : AbstractValidator<RecordTrainingAttendanceCommand>
{
    public RecordTrainingAttendanceCommandValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
        RuleFor(x => x.Entries).NotEmpty();
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.MemberId).NotEmpty();
        });
    }
}

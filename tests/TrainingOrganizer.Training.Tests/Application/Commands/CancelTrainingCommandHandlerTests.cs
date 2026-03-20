using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.Commands;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain.Enums;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Tests.Application.Commands;

public sealed class CancelTrainingCommandHandlerTests
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CancelTrainingCommandHandler _handler;

    public CancelTrainingCommandHandlerTests()
    {
        _trainingRepository = Substitute.For<ITrainingRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new CancelTrainingCommandHandler(_trainingRepository, _unitOfWork);
    }

    private static Domain.Training.Training CreateDraftTraining()
    {
        var trainerId = MemberId.Create();
        var createdBy = MemberId.Create();
        return Domain.Training.Training.Create(
            new TrainingTitle("Training to Cancel"),
            new TrainingDescription("Description"),
            new TimeSlot(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(1)),
            new Capacity(1, 20),
            Visibility.Public,
            [trainerId],
            createdBy);
    }

    [Fact]
    public async Task Handle_DraftTraining_ReturnsSuccess()
    {
        // Arrange
        var training = CreateDraftTraining();
        _trainingRepository.GetByIdAsync(Arg.Any<TrainingId>(), Arg.Any<CancellationToken>())
            .Returns(training);

        var command = new CancelTrainingCommand(training.Id.Value, "No longer needed");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TrainingNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _trainingRepository.GetByIdAsync(Arg.Any<TrainingId>(), Arg.Any<CancellationToken>())
            .Returns((Domain.Training.Training?)null);

        var command = new CancelTrainingCommand(Guid.NewGuid(), "Some reason");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryUpdate()
    {
        // Arrange
        var training = CreateDraftTraining();
        _trainingRepository.GetByIdAsync(Arg.Any<TrainingId>(), Arg.Any<CancellationToken>())
            .Returns(training);

        var command = new CancelTrainingCommand(training.Id.Value, "Cancelled due to weather");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _trainingRepository.Received(1)
            .UpdateAsync(Arg.Any<Domain.Training.Training>(), Arg.Any<CancellationToken>());
    }
}

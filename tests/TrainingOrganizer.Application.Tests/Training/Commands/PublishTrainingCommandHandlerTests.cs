using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Commands;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Application.Tests.Training.Commands;

public sealed class PublishTrainingCommandHandlerTests
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PublishTrainingCommandHandler _handler;

    public PublishTrainingCommandHandlerTests()
    {
        _trainingRepository = Substitute.For<ITrainingRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new PublishTrainingCommandHandler(_trainingRepository, _unitOfWork);
    }

    private static Domain.Training.Training CreateDraftTraining()
    {
        var trainerId = MemberId.Create();
        var createdBy = MemberId.Create();
        return Domain.Training.Training.Create(
            new TrainingTitle("Draft Training"),
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

        var command = new PublishTrainingCommand(training.Id.Value);

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

        var command = new PublishTrainingCommand(Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

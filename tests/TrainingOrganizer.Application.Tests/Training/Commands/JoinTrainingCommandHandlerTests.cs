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

public sealed class JoinTrainingCommandHandlerTests
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JoinTrainingCommandHandler _handler;

    public JoinTrainingCommandHandlerTests()
    {
        _trainingRepository = Substitute.For<ITrainingRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new JoinTrainingCommandHandler(_trainingRepository, _currentUserService, _unitOfWork);
    }

    private static Domain.Training.Training CreatePublishedTraining(int maxCapacity = 20)
    {
        var trainerId = MemberId.Create();
        var createdBy = MemberId.Create();
        var training = Domain.Training.Training.Create(
            new TrainingTitle("Test Training"),
            new TrainingDescription("Description"),
            new TimeSlot(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(1)),
            new Capacity(1, maxCapacity),
            Visibility.Public,
            [trainerId],
            createdBy);
        training.Publish();
        return training;
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId);

        var training = CreatePublishedTraining();
        _trainingRepository.GetByIdAsync(Arg.Any<TrainingId>(), Arg.Any<CancellationToken>())
            .Returns(training);

        var command = new JoinTrainingCommand(training.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TrainingNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId);

        _trainingRepository.GetByIdAsync(Arg.Any<TrainingId>(), Arg.Any<CancellationToken>())
            .Returns((Domain.Training.Training?)null);

        var command = new JoinTrainingCommand(Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CapacityFull_StillSucceeds()
    {
        // Arrange: create a training with max capacity of 1, add one participant, then try adding another
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId);

        var training = CreatePublishedTraining(maxCapacity: 1);
        // Fill the capacity with one participant
        var firstParticipant = MemberId.Create();
        training.AddParticipant(firstParticipant);

        _trainingRepository.GetByIdAsync(Arg.Any<TrainingId>(), Arg.Any<CancellationToken>())
            .Returns(training);

        var command = new JoinTrainingCommand(training.Id.Value);

        // Act - second participant goes to waitlist
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - still succeeds (participant is waitlisted)
        result.IsSuccess.Should().BeTrue();
    }
}

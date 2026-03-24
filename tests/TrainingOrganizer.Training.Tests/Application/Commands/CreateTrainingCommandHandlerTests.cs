using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.Commands;
using TrainingOrganizer.Training.Application.DTOs;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain.Enums;
using DomainTraining = TrainingOrganizer.Training.Domain.Training;

namespace TrainingOrganizer.Training.Tests.Application.Commands;

public sealed class CreateTrainingCommandHandlerTests
{
    private readonly ITrainingRepository _trainingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateTrainingCommandHandler _handler;

    public CreateTrainingCommandHandlerTests()
    {
        _trainingRepository = Substitute.For<ITrainingRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new CreateTrainingCommandHandler(_trainingRepository, _currentUserService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithTrainingId()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId.Value);

        var trainerId = Guid.NewGuid();
        var command = new CreateTrainingCommand(
            "Yoga Class",
            "A relaxing yoga session",
            Start: DateTimeOffset.UtcNow.AddDays(1),
            End: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            MinCapacity: 5,
            MaxCapacity: 20,
            Visibility: Visibility.Public,
            TrainerIds: [trainerId],
            RoomRequirements: []);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAdd()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId.Value);

        var trainerId = Guid.NewGuid();
        var command = new CreateTrainingCommand(
            "Pilates",
            null,
            Start: DateTimeOffset.UtcNow.AddDays(2),
            End: DateTimeOffset.UtcNow.AddDays(2).AddHours(1),
            MinCapacity: 3,
            MaxCapacity: 15,
            Visibility: Visibility.MembersOnly,
            TrainerIds: [trainerId],
            RoomRequirements: []);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _trainingRepository.Received(1)
            .AddAsync(Arg.Any<DomainTraining>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyTrainerIds_ReturnsFailure()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId.Value);

        var command = new CreateTrainingCommand(
            "Yoga Class",
            "A relaxing yoga session",
            Start: DateTimeOffset.UtcNow.AddDays(1),
            End: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            MinCapacity: 5,
            MaxCapacity: 20,
            Visibility: Visibility.Public,
            TrainerIds: [],
            RoomRequirements: []);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Training.DomainError");
    }

    [Fact]
    public async Task Handle_WithRoomRequirements_ReturnsSuccessAndAddsRooms()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId.Value);

        var trainerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var command = new CreateTrainingCommand(
            "Yoga Class",
            "A relaxing yoga session",
            Start: DateTimeOffset.UtcNow.AddDays(1),
            End: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            MinCapacity: 5,
            MaxCapacity: 20,
            Visibility: Visibility.Public,
            TrainerIds: [trainerId],
            RoomRequirements: [new RoomRequirementDto(roomId, locationId)]);

        DomainTraining? savedTraining = null;
        await _trainingRepository.AddAsync(
            Arg.Do<DomainTraining>(t => savedTraining = t),
            Arg.Any<CancellationToken>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        savedTraining.Should().NotBeNull();
        savedTraining!.RoomRequirements.Should().HaveCount(1);
        savedTraining.RoomRequirements[0].RoomId.Value.Should().Be(roomId);
        savedTraining.RoomRequirements[0].LocationId.Value.Should().Be(locationId);
    }

    [Fact]
    public async Task Handle_WithEmptyRoomRequirements_ReturnsSuccessWithNoRooms()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId.Value);

        var trainerId = Guid.NewGuid();
        var command = new CreateTrainingCommand(
            "Outdoor Running",
            null,
            Start: DateTimeOffset.UtcNow.AddDays(1),
            End: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            MinCapacity: 1,
            MaxCapacity: 30,
            Visibility: Visibility.Public,
            TrainerIds: [trainerId],
            RoomRequirements: []);

        DomainTraining? savedTraining = null;
        await _trainingRepository.AddAsync(
            Arg.Do<DomainTraining>(t => savedTraining = t),
            Arg.Any<CancellationToken>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        savedTraining.Should().NotBeNull();
        savedTraining!.RoomRequirements.Should().BeEmpty();
    }
}

using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Training.Commands;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;

namespace TrainingOrganizer.Application.Tests.Training.Commands;

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
        _currentUserService.MemberId.Returns(currentUserId);

        var trainerId = Guid.NewGuid();
        var command = new CreateTrainingCommand(
            "Yoga Class",
            "A relaxing yoga session",
            Start: DateTimeOffset.UtcNow.AddDays(1),
            End: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            MinCapacity: 5,
            MaxCapacity: 20,
            Visibility: Visibility.Public,
            TrainerIds: [trainerId]);

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
        _currentUserService.MemberId.Returns(currentUserId);

        var trainerId = Guid.NewGuid();
        var command = new CreateTrainingCommand(
            "Pilates",
            null,
            Start: DateTimeOffset.UtcNow.AddDays(2),
            End: DateTimeOffset.UtcNow.AddDays(2).AddHours(1),
            MinCapacity: 3,
            MaxCapacity: 15,
            Visibility: Visibility.MembersOnly,
            TrainerIds: [trainerId]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _trainingRepository.Received(1)
            .AddAsync(Arg.Any<Domain.Training.Training>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyTrainerIds_ReturnsFailure()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId);

        var command = new CreateTrainingCommand(
            "Yoga Class",
            "A relaxing yoga session",
            Start: DateTimeOffset.UtcNow.AddDays(1),
            End: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            MinCapacity: 5,
            MaxCapacity: 20,
            Visibility: Visibility.Public,
            TrainerIds: []);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Training.DomainError");
    }
}

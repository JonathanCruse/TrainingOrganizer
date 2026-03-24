using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Training.Application.Commands;
using TrainingOrganizer.Training.Application.DTOs;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain;
using TrainingOrganizer.Training.Domain.Enums;

namespace TrainingOrganizer.Training.Tests.Application.Commands;

public sealed class CreateRecurringTrainingCommandHandlerTests
{
    private readonly IRecurringTrainingRepository _recurringTrainingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateRecurringTrainingCommandHandler _handler;

    public CreateRecurringTrainingCommandHandlerTests()
    {
        _recurringTrainingRepository = Substitute.For<IRecurringTrainingRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new CreateRecurringTrainingCommandHandler(_recurringTrainingRepository, _currentUserService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithRecurringTrainingId()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId.Value);

        var trainerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var command = new CreateRecurringTrainingCommand(
            "Weekly Yoga",
            "Recurring yoga class",
            MinCapacity: 5,
            MaxCapacity: 20,
            Visibility: Visibility.Public,
            TrainerIds: [trainerId],
            RoomRequirements: [new RoomRequirementDto(roomId, locationId)],
            Pattern: RecurrencePattern.Weekly,
            DayOfWeek: DayOfWeek.Monday,
            TimeOfDay: "10:00",
            Duration: "01:00",
            StartDate: "2026-04-01",
            EndDate: "2026-06-30");

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
        var command = new CreateRecurringTrainingCommand(
            "Biweekly Pilates",
            null,
            MinCapacity: 3,
            MaxCapacity: 15,
            Visibility: Visibility.MembersOnly,
            TrainerIds: [trainerId],
            RoomRequirements: [],
            Pattern: RecurrencePattern.Biweekly,
            DayOfWeek: DayOfWeek.Wednesday,
            TimeOfDay: "14:00",
            Duration: "01:30",
            StartDate: "2026-04-01",
            EndDate: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _recurringTrainingRepository.Received(1)
            .AddAsync(Arg.Any<RecurringTraining>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyTrainerIds_ReturnsFailure()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId.Value);

        var command = new CreateRecurringTrainingCommand(
            "Weekly Yoga",
            "Description",
            MinCapacity: 5,
            MaxCapacity: 20,
            Visibility: Visibility.Public,
            TrainerIds: [],
            RoomRequirements: [],
            Pattern: RecurrencePattern.Weekly,
            DayOfWeek: DayOfWeek.Monday,
            TimeOfDay: "10:00",
            Duration: "01:00",
            StartDate: "2026-04-01",
            EndDate: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RecurringTraining.DomainError");
    }
}

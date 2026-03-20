using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Application.Commands;
using TrainingOrganizer.Facility.Application.Repositories;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Tests.Application.Commands;

public sealed class AddRoomCommandHandlerTests
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AddRoomCommandHandler _handler;

    public AddRoomCommandHandlerTests()
    {
        _locationRepository = Substitute.For<ILocationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new AddRoomCommandHandler(_locationRepository, _unitOfWork);
    }

    private static Location CreateLocation()
    {
        return Location.Create(new LocationName("Sports Center"), new Address("Main St", "Berlin", "10115", "Germany"));
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithRoomId()
    {
        // Arrange
        var location = CreateLocation();
        _locationRepository.GetByIdAsync(Arg.Any<LocationId>(), Arg.Any<CancellationToken>())
            .Returns(location);

        var command = new AddRoomCommand(location.Id.Value, "Yoga Room", 30);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_LocationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _locationRepository.GetByIdAsync(Arg.Any<LocationId>(), Arg.Any<CancellationToken>())
            .Returns((Location?)null);

        var command = new AddRoomCommand(Guid.NewGuid(), "Yoga Room", 30);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DuplicateRoomName_ReturnsFailure()
    {
        // Arrange
        var location = CreateLocation();
        location.AddRoom(new RoomName("Yoga Room"), 20);

        _locationRepository.GetByIdAsync(Arg.Any<LocationId>(), Arg.Any<CancellationToken>())
            .Returns(location);

        var command = new AddRoomCommand(location.Id.Value, "Yoga Room", 30);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Room.DomainError");
    }
}

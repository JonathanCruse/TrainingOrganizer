using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Application.Commands;
using TrainingOrganizer.Facility.Application.Repositories;
using TrainingOrganizer.Facility.Domain;

namespace TrainingOrganizer.Facility.Tests.Application.Commands;

public sealed class CreateLocationCommandHandlerTests
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateLocationCommandHandler _handler;

    public CreateLocationCommandHandlerTests()
    {
        _locationRepository = Substitute.For<ILocationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new CreateLocationCommandHandler(_locationRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithLocationId()
    {
        // Arrange
        var command = new CreateLocationCommand("Sports Center", "Main Street 1", "Berlin", "10115", "Germany");

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
        var command = new CreateLocationCommand("Yoga Studio", "Park Ave 42", "Munich", "80331", "Germany");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _locationRepository.Received(1).AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }
}

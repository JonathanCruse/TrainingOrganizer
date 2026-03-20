using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Application.Commands;
using TrainingOrganizer.Facility.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Facility.Domain.Services;

namespace TrainingOrganizer.Facility.Tests.Application.Commands;

public sealed class CreateBookingCommandHandlerTests
{
    private readonly IRoomBookingService _roomBookingService;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateBookingCommandHandler _handler;

    public CreateBookingCommandHandlerTests()
    {
        _roomBookingService = Substitute.For<IRoomBookingService>();
        _bookingRepository = Substitute.For<IBookingRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new CreateBookingCommandHandler(_roomBookingService, _bookingRepository, _currentUserService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_NoConflict_ReturnsSuccessWithBookingId()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId);

        _roomBookingService.HasConflictAsync(
                Arg.Any<RoomId>(), Arg.Any<TimeSlot>(), Arg.Any<BookingId?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        var referenceId = Guid.NewGuid();

        var command = new CreateBookingCommand(
            RoomId: Guid.NewGuid(),
            LocationId: Guid.NewGuid(),
            Start: start,
            End: end,
            ReferenceType: BookingReferenceType.Training,
            ReferenceId: referenceId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithConflict_ReturnsFailure()
    {
        // Arrange
        var currentUserId = MemberId.Create();
        _currentUserService.MemberId.Returns(currentUserId);

        _roomBookingService.HasConflictAsync(
                Arg.Any<RoomId>(), Arg.Any<TimeSlot>(), Arg.Any<BookingId?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        var referenceId = Guid.NewGuid();

        var command = new CreateBookingCommand(
            RoomId: Guid.NewGuid(),
            LocationId: Guid.NewGuid(),
            Start: start,
            End: end,
            ReferenceType: BookingReferenceType.Training,
            ReferenceId: referenceId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Conflict");
    }
}

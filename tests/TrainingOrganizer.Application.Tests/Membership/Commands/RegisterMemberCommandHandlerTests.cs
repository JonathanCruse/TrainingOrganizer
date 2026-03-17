using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Membership.Commands;
using TrainingOrganizer.Application.Membership.Repositories;
using TrainingOrganizer.Domain.Membership;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Services;

namespace TrainingOrganizer.Application.Tests.Membership.Commands;

public sealed class RegisterMemberCommandHandlerTests
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberUniquenessService _memberUniquenessService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegisterMemberCommandHandler _handler;

    public RegisterMemberCommandHandlerTests()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _memberUniquenessService = Substitute.For<IMemberUniquenessService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new RegisterMemberCommandHandler(_memberRepository, _memberUniquenessService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithMemberId()
    {
        // Arrange
        _memberUniquenessService.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<MemberId?>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var command = new RegisterMemberCommand("keycloak", "sub-123", "Jane", "Doe", "jane@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        _memberUniquenessService.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<MemberId?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        var command = new RegisterMemberCommand("keycloak", "sub-123", "Jane", "Doe", "jane@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Member.DuplicateEmail");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAdd()
    {
        // Arrange
        _memberUniquenessService.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<MemberId?>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var command = new RegisterMemberCommand("keycloak", "sub-456", "John", "Smith", "john@example.com");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _memberRepository.Received(1).AddAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
    }
}

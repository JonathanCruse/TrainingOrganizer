using FluentAssertions;
using NSubstitute;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.Commands;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.Membership.Domain;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Tests.Application.Commands;

public sealed class ApproveMemberCommandHandlerTests
{
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApproveMemberCommandHandler _handler;

    public ApproveMemberCommandHandlerTests()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        _handler = new ApproveMemberCommandHandler(_memberRepository, _currentUserService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var approverId = MemberId.Create();
        _currentUserService.MemberId.Returns(approverId);

        var member = Member.Register(
            new ExternalIdentity("keycloak", "sub-1"),
            new PersonName("Jane", "Doe"),
            new Email("jane@example.com"));

        _memberRepository.GetByIdAsync(Arg.Any<MemberId>(), Arg.Any<CancellationToken>())
            .Returns(member);

        var command = new ApproveMemberCommand(member.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var approverId = MemberId.Create();
        _currentUserService.MemberId.Returns(approverId);

        _memberRepository.GetByIdAsync(Arg.Any<MemberId>(), Arg.Any<CancellationToken>())
            .Returns((Member?)null);

        var command = new ApproveMemberCommand(Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryUpdate()
    {
        // Arrange
        var approverId = MemberId.Create();
        _currentUserService.MemberId.Returns(approverId);

        var member = Member.Register(
            new ExternalIdentity("keycloak", "sub-2"),
            new PersonName("John", "Smith"),
            new Email("john@example.com"));

        _memberRepository.GetByIdAsync(Arg.Any<MemberId>(), Arg.Any<CancellationToken>())
            .Returns(member);

        var command = new ApproveMemberCommand(member.Id.Value);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _memberRepository.Received(1).UpdateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
    }
}

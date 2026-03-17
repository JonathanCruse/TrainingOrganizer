using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Shared.Enums;
using TrainingOrganizer.Shared.Models;
using TrainingOrganizer.UI.Pages.Members;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Components;

public sealed class MemberListTests : BunitTestBase
{
    private readonly MockHttpMessageHandler _handler = new();

    public MemberListTests()
    {
        var httpClient = CreateMockHttpClient(_handler);
        Services.AddSingleton(new MemberApiClient(httpClient));
    }

    [Fact]
    public void RendersMemberData_WhenLoaded()
    {
        var members = CreatePagedResponse(
            new MemberResponse(
                Guid.NewGuid(), "John", "Doe", "john@test.com", "+49123",
                [MemberRole.Member], RegistrationStatus.Approved,
                DateTimeOffset.UtcNow));

        _handler.RespondWithJson("/api/v1/members?page=1&pageSize=20", members);

        var cut = Render<MemberList>();

        cut.Markup.Should().Contain("John");
        cut.Markup.Should().Contain("Doe");
        cut.Markup.Should().Contain("john@test.com");
    }

    [Fact]
    public void RendersAllRoles_AsChips()
    {
        var members = CreatePagedResponse(
            new MemberResponse(
                Guid.NewGuid(), "Jane", "Multi", "jane@test.com", null,
                [MemberRole.Member, MemberRole.Trainer, MemberRole.Admin],
                RegistrationStatus.Approved,
                DateTimeOffset.UtcNow));

        _handler.RespondWithJson("/api/v1/members?page=1&pageSize=20", members);

        var cut = Render<MemberList>();

        cut.Markup.Should().Contain("Member");
        cut.Markup.Should().Contain("Trainer");
        cut.Markup.Should().Contain("Admin");
    }

    [Fact]
    public void RendersStatusText_ForEachStatus()
    {
        var members = CreatePagedResponse(
            new MemberResponse(
                Guid.NewGuid(), "Pending", "User", "pending@test.com", null,
                [MemberRole.Member], RegistrationStatus.Pending,
                DateTimeOffset.UtcNow),
            new MemberResponse(
                Guid.NewGuid(), "Approved", "User", "approved@test.com", null,
                [MemberRole.Member], RegistrationStatus.Approved,
                DateTimeOffset.UtcNow));

        _handler.RespondWithJson("/api/v1/members?page=1&pageSize=20", members);

        var cut = Render<MemberList>();

        cut.Markup.Should().Contain("Pending");
        cut.Markup.Should().Contain("Approved");
    }

    private static PagedResponse<MemberResponse> CreatePagedResponse(params MemberResponse[] items)
    {
        return new PagedResponse<MemberResponse>(
            Items: items,
            Page: 1,
            PageSize: 20,
            TotalCount: items.Length,
            TotalPages: 1,
            HasNextPage: false,
            HasPreviousPage: false);
    }
}

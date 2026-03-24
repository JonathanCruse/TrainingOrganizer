using System.Net;
using FluentAssertions;
using TrainingOrganizer.Shared.Enums;
using TrainingOrganizer.Shared.Models;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Services;

public sealed class MemberApiClientTests
{
    private readonly MockHttpMessageHandler _handler = new();
    private readonly MemberApiClient _sut;

    public MemberApiClientTests()
    {
        var httpClient = new HttpClient(_handler) { BaseAddress = new Uri("http://localhost/") };
        _sut = new MemberApiClient(httpClient);
    }

    [Fact]
    public async Task GetAllAsync_SendsGetWithPagination()
    {
        var response = new PagedResponse<MemberResponse>(
            Items: [],
            Page: 2,
            PageSize: 10,
            TotalCount: 0,
            TotalPages: 0,
            HasNextPage: false,
            HasPreviousPage: true);

        _handler.RespondWithJson("/api/v1/members?page=2&pageSize=10", response);

        var result = await _sut.GetAllAsync(page: 2, pageSize: 10);

        result.Should().NotBeNull();
        result!.Page.Should().Be(2);
        _handler.SentRequests.Should().ContainSingle()
            .Which.Method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public async Task GetByIdAsync_SendsGetWithId()
    {
        var id = Guid.NewGuid();
        var member = new MemberResponse(
            id, "John", "Doe", "john@test.com", null,
            [MemberRole.Member], RegistrationStatus.Approved,
            DateTimeOffset.UtcNow);

        _handler.RespondWithJson($"/api/v1/members/{id}", member);

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task RegisterAsync_PostsToCorrectUrl()
    {
        _handler.RespondWith("/api/v1/members/register", HttpStatusCode.Created);

        var request = new RegisterMemberRequest("John", "Doe", "john@test.com");
        var result = await _sut.RegisterAsync(request);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        _handler.SentRequests.Should().ContainSingle()
            .Which.Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task ApproveAsync_PostsWithoutBody()
    {
        var id = Guid.NewGuid();
        _handler.RespondWith($"/api/v1/members/{id}/approve", HttpStatusCode.OK);

        var result = await _sut.ApproveAsync(id);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var request = _handler.SentRequests.Should().ContainSingle().Subject;
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.PathAndQuery.Should().Contain($"/api/v1/members/{id}/approve");
    }

    [Fact]
    public async Task RemoveRoleAsync_SendsDeleteRequest()
    {
        var id = Guid.NewGuid();
        _handler.RespondWith($"/api/v1/members/{id}/roles/Trainer", HttpStatusCode.OK);

        var result = await _sut.RemoveRoleAsync(id, "Trainer");

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        _handler.SentRequests.Should().ContainSingle()
            .Which.Method.Should().Be(HttpMethod.Delete);
    }
}

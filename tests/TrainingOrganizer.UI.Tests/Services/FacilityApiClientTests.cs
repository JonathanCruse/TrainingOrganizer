using System.Net;
using FluentAssertions;
using TrainingOrganizer.Shared.Enums;
using TrainingOrganizer.Shared.Models;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Services;

public sealed class FacilityApiClientTests
{
    private readonly MockHttpMessageHandler _handler = new();
    private readonly FacilityApiClient _client;

    public FacilityApiClientTests()
    {
        var httpClient = new HttpClient(_handler) { BaseAddress = new Uri("http://localhost/") };
        _client = new FacilityApiClient(httpClient);
    }

    [Fact]
    public async Task GetBookingsAsync_SendsGetRequest_WithFilters()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var from = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 4, 7, 23, 59, 59, TimeSpan.Zero);

        _handler.RespondWithJson("/api/v1/bookings", new PagedResponse<BookingResponse>(
            [], 1, 20, 0, 0, false, false));

        // Act
        var result = await _client.GetBookingsAsync(1, 20, roomId, from, to);

        // Assert
        _handler.SentRequests.Should().ContainSingle();
        var request = _handler.SentRequests[0];
        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri!.PathAndQuery.Should().Contain("/api/v1/bookings");
        request.RequestUri.PathAndQuery.Should().Contain($"roomId={roomId}");
    }

    [Fact]
    public async Task GetBookingsAsync_WithoutFilters_SendsBaseUrl()
    {
        // Arrange
        _handler.RespondWithJson("/api/v1/bookings", new PagedResponse<BookingResponse>(
            [], 1, 20, 0, 0, false, false));

        // Act
        var result = await _client.GetBookingsAsync();

        // Assert
        _handler.SentRequests.Should().ContainSingle();
        var request = _handler.SentRequests[0];
        request.RequestUri!.PathAndQuery.Should().Be("/api/v1/bookings?page=1&pageSize=20");
    }

    [Fact]
    public async Task CreateBookingAsync_SendsPostRequest()
    {
        // Arrange
        _handler.RespondWith("/api/v1/bookings", HttpStatusCode.Created);

        var request = new CreateBookingRequest(
            Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1),
            "Manual", Guid.NewGuid());

        // Act
        var response = await _client.CreateBookingAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _handler.SentRequests.Should().ContainSingle()
            .Which.Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task CancelBookingAsync_SendsPostRequest()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        _handler.RespondWith($"/api/v1/bookings/{bookingId}/cancel", HttpStatusCode.OK);

        // Act
        var response = await _client.CancelBookingAsync(bookingId);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        _handler.SentRequests.Should().ContainSingle()
            .Which.RequestUri!.PathAndQuery.Should().Contain($"/api/v1/bookings/{bookingId}/cancel");
    }

    [Fact]
    public async Task RescheduleBookingAsync_SendsPutRequest()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        _handler.RespondWith($"/api/v1/bookings/{bookingId}/reschedule", HttpStatusCode.OK);

        var request = new RescheduleBookingRequest(
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(1).AddHours(1));

        // Act
        var response = await _client.RescheduleBookingAsync(bookingId, request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        _handler.SentRequests.Should().ContainSingle()
            .Which.Method.Should().Be(HttpMethod.Put);
    }

    [Fact]
    public async Task GetRoomAvailabilityAsync_SendsGetRequest()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        _handler.RespondWithJson($"/api/v1/bookings/rooms/{roomId}/availability",
            new List<TimeSlotResponse>());

        var from = new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 4, 1, 17, 0, 0, TimeSpan.Zero);

        // Act
        var result = await _client.GetRoomAvailabilityAsync(roomId, from, to);

        // Assert
        result.Should().BeEmpty();
        _handler.SentRequests.Should().ContainSingle()
            .Which.RequestUri!.PathAndQuery.Should().Contain($"/api/v1/bookings/rooms/{roomId}/availability");
    }
}

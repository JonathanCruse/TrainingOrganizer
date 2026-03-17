using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Shared.Enums;
using TrainingOrganizer.Shared.Models;
using TrainingOrganizer.UI.Pages.Facility;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Components;

public sealed class LocationListTests : BunitTestBase
{
    private readonly MockHttpMessageHandler _handler = new();

    public LocationListTests()
    {
        var httpClient = CreateMockHttpClient(_handler);
        Services.AddSingleton(new FacilityApiClient(httpClient));
    }

    [Fact]
    public void RendersLocationCards_WithAddressAndRooms()
    {
        var locations = CreatePagedResponse(
            new LocationResponse(
                Guid.NewGuid(),
                "Main Gym",
                "Musterstr. 1",
                "Berlin",
                "10115",
                "Germany",
                [
                    new RoomResponse(Guid.NewGuid(), "Studio A", 30, RoomStatus.Enabled),
                    new RoomResponse(Guid.NewGuid(), "Studio B", 15, RoomStatus.Enabled)
                ]));

        _handler.RespondWithJson("/api/v1/locations?page=1&pageSize=20", locations);

        var cut = Render<LocationList>();

        cut.Markup.Should().Contain("Main Gym");
        cut.Markup.Should().Contain("Musterstr. 1");
        cut.Markup.Should().Contain("Berlin");
        cut.Markup.Should().Contain("10115");
        cut.Markup.Should().Contain("Studio A");
        cut.Markup.Should().Contain("30");
        cut.Markup.Should().Contain("Studio B");
        cut.Markup.Should().Contain("15");
    }

    [Fact]
    public void RendersLocationWithNoRooms()
    {
        var locations = CreatePagedResponse(
            new LocationResponse(
                Guid.NewGuid(),
                "Empty Location",
                "Teststr. 5",
                "Munich",
                "80331",
                "Germany",
                []));

        _handler.RespondWithJson("/api/v1/locations?page=1&pageSize=20", locations);

        var cut = Render<LocationList>();

        cut.Markup.Should().Contain("Empty Location");
        cut.Markup.Should().Contain("Rooms (0)");
    }

    private static PagedResponse<LocationResponse> CreatePagedResponse(params LocationResponse[] items)
    {
        return new PagedResponse<LocationResponse>(
            Items: items,
            Page: 1,
            PageSize: 20,
            TotalCount: items.Length,
            TotalPages: 1,
            HasNextPage: false,
            HasPreviousPage: false);
    }
}

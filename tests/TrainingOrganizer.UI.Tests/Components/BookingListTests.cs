using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using TrainingOrganizer.Shared.Enums;
using TrainingOrganizer.Shared.Models;
using TrainingOrganizer.UI.Pages.Facility;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Components;

public sealed class BookingListTests : BunitTestBase
{
    private readonly MockHttpMessageHandler _handler = new();
    private readonly Guid _memberId = Guid.NewGuid();

    public BookingListTests()
    {
        var httpClient = CreateMockHttpClient(_handler);
        Services.AddSingleton(new FacilityApiClient(httpClient));

        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@test.com");
        authContext.SetRoles("Admin");
        authContext.SetClaims(new Claim("member_id", _memberId.ToString()));

        // Mock both endpoints since component loads both on init
        _handler.RespondWithJson("/api/v1/locations", CreatePagedResponse<LocationResponse>());
        _handler.RespondWithJson("/api/v1/bookings", CreatePagedResponse<BookingResponse>());

        Render<MudPopoverProvider>();
    }

    [Fact]
    public void RendersPageTitle()
    {
        var cut = Render<BookingList>();
        cut.Markup.Should().Contain("Room Bookings");
    }

    [Fact]
    public void RendersNewBookingButton_ForAdmin()
    {
        var cut = Render<BookingList>();
        cut.Markup.Should().Contain("New Booking");
    }

    [Fact]
    public void RendersFilterControls()
    {
        var cut = Render<BookingList>();
        cut.Markup.Should().Contain("Location");
        cut.Markup.Should().Contain("Room");
        cut.Markup.Should().Contain("From");
        cut.Markup.Should().Contain("To");
    }

    private static PagedResponse<T> CreatePagedResponse<T>(params T[] items)
    {
        return new PagedResponse<T>(
            Items: items,
            Page: 1,
            PageSize: 100,
            TotalCount: items.Length,
            TotalPages: 1,
            HasNextPage: false,
            HasPreviousPage: false);
    }
}

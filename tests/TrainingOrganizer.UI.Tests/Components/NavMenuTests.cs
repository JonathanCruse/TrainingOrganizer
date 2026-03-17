using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using TrainingOrganizer.UI.Layout;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Components;

public sealed class NavMenuTests : BunitTestBase
{
    [Fact]
    public void Unauthenticated_ShowsOnlyPublicLinks()
    {
        this.AddAuthorization();

        var cut = Render<NavMenu>();

        cut.Markup.Should().Contain("Home");
        cut.Markup.Should().Contain("My Schedule");
        cut.Markup.Should().Contain("Trainings");
        cut.Markup.Should().NotContain("/members");
        cut.Markup.Should().NotContain("/locations");
    }

    [Fact]
    public void RegularMember_DoesNotSeeAdminLinks()
    {
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("user@test.com");
        authContext.SetRoles("Member");

        var cut = Render<NavMenu>();

        cut.Markup.Should().Contain("Home");
        cut.Markup.Should().Contain("Trainings");
        cut.Markup.Should().NotContain("/members");
        cut.Markup.Should().NotContain("/locations");
    }

    [Theory]
    [InlineData("Trainer")]
    [InlineData("Admin")]
    public void TrainerOrAdmin_SeesAllLinks(string role)
    {
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@test.com");
        authContext.SetRoles(role);

        var cut = Render<NavMenu>();

        cut.Markup.Should().Contain("Home");
        cut.Markup.Should().Contain("My Schedule");
        cut.Markup.Should().Contain("Trainings");
        cut.Markup.Should().Contain("/members");
        cut.Markup.Should().Contain("/locations");
    }
}

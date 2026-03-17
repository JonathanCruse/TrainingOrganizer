using Bunit;
using MudBlazor.Services;

namespace TrainingOrganizer.UI.Tests.Helpers;

public abstract class BunitTestBase : BunitContext, IAsyncLifetime
{
    protected BunitTestBase()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    protected HttpClient CreateMockHttpClient(MockHttpMessageHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
    }
}

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

// Configure HttpClient with auth token attached automatically
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.ConfigureHandler(authorizedUrls: [apiBaseUrl]);
    return handler;
});

// Register the typed HttpClient for DI (used by API client services)
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

// Add OIDC authentication for Keycloak
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Oidc", options.ProviderOptions);
    options.ProviderOptions.PostLogoutRedirectUri = builder.HostEnvironment.BaseAddress;
    // Keycloak puts roles in the "roles" claim (configured via protocol mapper)
    options.UserOptions.RoleClaim = "roles";
    // Map Keycloak's "preferred_username" to the Name claim
    options.UserOptions.NameClaim = "preferred_username";
}).AddAccountClaimsPrincipalFactory<RolesClaimsPrincipalFactory>();

// Add MudBlazor
builder.Services.AddMudServices();

// Add API clients
builder.Services.AddUIServices();

await builder.Build().RunAsync();

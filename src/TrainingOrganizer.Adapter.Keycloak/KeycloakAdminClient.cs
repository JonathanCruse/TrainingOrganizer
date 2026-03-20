using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrainingOrganizer.Membership.Application.Services;

namespace TrainingOrganizer.Adapter.Keycloak;

public sealed class KeycloakAdminClient : IKeycloakAdminClient
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakAdminSettings _settings;
    private readonly ILogger<KeycloakAdminClient> _logger;
    private string? _accessToken;
    private DateTimeOffset _tokenExpiry;

    public KeycloakAdminClient(
        HttpClient httpClient,
        IOptions<KeycloakAdminSettings> settings,
        ILogger<KeycloakAdminClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<string> CreateOrGetUserAsync(string email, string firstName, string lastName, CancellationToken ct = default)
    {
        await EnsureAuthenticatedAsync(ct);

        // Check if user already exists
        var existingUserId = await FindUserByEmailAsync(email, ct);
        if (existingUserId is not null)
        {
            _logger.LogDebug("Keycloak user already exists for {Email}: {UserId}", email, existingUserId);
            return existingUserId;
        }

        // Create new user
        var userRepresentation = new
        {
            username = email,
            email,
            firstName,
            lastName,
            enabled = true,
            emailVerified = true,
            requiredActions = new[] { "UPDATE_PASSWORD" }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"admin/realms/{_settings.Realm}/users", userRepresentation, ct);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            // Race condition: user was created between check and create
            var userId = await FindUserByEmailAsync(email, ct);
            return userId ?? throw new InvalidOperationException($"Keycloak user creation returned 409 but user not found for {email}");
        }

        response.EnsureSuccessStatusCode();

        // Extract user ID from Location header
        var location = response.Headers.Location?.ToString()
            ?? throw new InvalidOperationException("Keycloak did not return a Location header after user creation.");

        var createdUserId = location.Split('/').Last();
        _logger.LogInformation("Created Keycloak user {UserId} for {Email}", createdUserId, email);
        return createdUserId;
    }

    public async Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        await EnsureAuthenticatedAsync(ct);

        // Get role representation
        var roleResponse = await _httpClient.GetAsync(
            $"admin/realms/{_settings.Realm}/roles/{roleName}", ct);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<KeycloakRole>(ct)
            ?? throw new InvalidOperationException($"Could not deserialize role '{roleName}' from Keycloak.");

        // Assign role to user
        var response = await _httpClient.PostAsJsonAsync(
            $"admin/realms/{_settings.Realm}/users/{userId}/role-mappings/realm",
            new[] { role }, ct);

        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Assigned realm role '{Role}' to Keycloak user {UserId}", roleName, userId);
    }

    private async Task<string?> FindUserByEmailAsync(string email, CancellationToken ct)
    {
        var users = await _httpClient.GetFromJsonAsync<List<KeycloakUser>>(
            $"admin/realms/{_settings.Realm}/users?email={Uri.EscapeDataString(email)}&exact=true", ct);

        return users?.FirstOrDefault()?.Id;
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken ct)
    {
        if (_accessToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            return;
        }

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = _settings.AdminUsername,
            ["password"] = _settings.AdminPassword
        });

        var response = await _httpClient.PostAsync(
            "realms/master/protocol/openid-connect/token", tokenRequest, ct);
        response.EnsureSuccessStatusCode();

        var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize Keycloak token response.");

        _accessToken = tokenResult.AccessToken;
        _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(tokenResult.ExpiresIn - 30);

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record KeycloakUser(
        [property: JsonPropertyName("id")] string Id);

    private sealed record KeycloakRole(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name);
}

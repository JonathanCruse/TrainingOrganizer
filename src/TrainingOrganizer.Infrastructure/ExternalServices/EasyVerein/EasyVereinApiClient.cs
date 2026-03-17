using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrainingOrganizer.Application.Membership.Services;

namespace TrainingOrganizer.Infrastructure.ExternalServices.EasyVerein;

public sealed class EasyVereinApiClient : IEasyVereinApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EasyVereinApiClient> _logger;

    public EasyVereinApiClient(HttpClient httpClient, IOptions<EasyVereinSettings> settings, ILogger<EasyVereinApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var config = settings.Value;
        _httpClient.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiToken);
    }

    public async Task<List<EasyVereinMemberDto>> GetAllMembersAsync(CancellationToken ct = default)
    {
        var allMembers = new List<EasyVereinMemberDto>();
        var query = Uri.EscapeDataString("{id,emailOrUserName,membershipNumber,contactDetails{id,firstName,familyName},memberGroups{id,name}}");
        var url = $"member?query={query}&limit=100";

        while (url is not null)
        {
            _logger.LogDebug("Fetching EasyVerein members: {Url}", url);

            var response = await _httpClient.GetFromJsonAsync<EasyVereinPagedResponse<EasyVereinApiMember>>(url, ct)
                ?? throw new InvalidOperationException("EasyVerein API returned null response.");

            foreach (var apiMember in response.Results)
            {
                allMembers.Add(MapToDto(apiMember));
            }

            url = GetNextRelativeUrl(response.Next);
        }

        _logger.LogInformation("Fetched {Count} members from EasyVerein", allMembers.Count);
        return allMembers;
    }

    public async Task<List<EasyVereinMemberGroupDto>> GetAllMemberGroupsAsync(CancellationToken ct = default)
    {
        var allGroups = new List<EasyVereinMemberGroupDto>();
        var url = "member-group?limit=100";

        while (url is not null)
        {
            var response = await _httpClient.GetFromJsonAsync<EasyVereinPagedResponse<EasyVereinApiMemberGroup>>(url, ct)
                ?? throw new InvalidOperationException("EasyVerein API returned null response.");

            foreach (var group in response.Results)
            {
                allGroups.Add(new EasyVereinMemberGroupDto
                {
                    Id = group.Id,
                    Name = group.Name ?? string.Empty,
                    Short = group.Short
                });
            }

            url = GetNextRelativeUrl(response.Next);
        }

        _logger.LogInformation("Fetched {Count} member groups from EasyVerein", allGroups.Count);
        return allGroups;
    }

    private static EasyVereinMemberDto MapToDto(EasyVereinApiMember api) => new()
    {
        Id = api.Id,
        EmailOrUserName = api.EmailOrUserName,
        MembershipNumber = api.MembershipNumber,
        ContactDetails = api.ContactDetails is not null
            ? new EasyVereinContactDetailsDto
            {
                Id = api.ContactDetails.Id,
                FirstName = api.ContactDetails.FirstName,
                FamilyName = api.ContactDetails.FamilyName
            }
            : null,
        MemberGroups = api.MemberGroups?.Select(g => new EasyVereinMemberGroupRefDto
        {
            Id = g.Id,
            Name = g.Name
        }).ToList()
    };

    private static string? GetNextRelativeUrl(string? nextUrl)
    {
        if (string.IsNullOrEmpty(nextUrl))
            return null;

        // The API returns absolute URLs for pagination. Extract the relative path.
        if (Uri.TryCreate(nextUrl, UriKind.Absolute, out var uri))
            return uri.PathAndQuery.TrimStart('/').Replace("api/v2.0/", "");

        return nextUrl;
    }

    // Internal API response models
    private sealed record EasyVereinPagedResponse<T>(
        [property: JsonPropertyName("count")] int Count,
        [property: JsonPropertyName("next")] string? Next,
        [property: JsonPropertyName("previous")] string? Previous,
        [property: JsonPropertyName("results")] List<T> Results);

    private sealed record EasyVereinApiMember(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("emailOrUserName")] string? EmailOrUserName,
        [property: JsonPropertyName("membershipNumber")] string? MembershipNumber,
        [property: JsonPropertyName("contactDetails")] EasyVereinApiContactDetails? ContactDetails,
        [property: JsonPropertyName("memberGroups")] List<EasyVereinApiMemberGroupRef>? MemberGroups);

    private sealed record EasyVereinApiContactDetails(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("firstName")] string? FirstName,
        [property: JsonPropertyName("familyName")] string? FamilyName);

    private sealed record EasyVereinApiMemberGroupRef(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string? Name);

    private sealed record EasyVereinApiMemberGroup(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("short")] string? Short);
}

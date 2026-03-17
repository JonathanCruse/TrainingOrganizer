using System.Net.Http.Json;
using TrainingOrganizer.Shared.Models;

namespace TrainingOrganizer.UI.Services;

public sealed class MemberApiClient(HttpClient http)
{
    public async Task<PagedResponse<MemberResponse>?> GetAllAsync(int page = 1, int pageSize = 20)
        => await http.GetFromJsonAsync<PagedResponse<MemberResponse>>($"api/v1/members?page={page}&pageSize={pageSize}");

    public async Task<MemberResponse?> GetByIdAsync(Guid id)
        => await http.GetFromJsonAsync<MemberResponse>($"api/v1/members/{id}");

    public async Task<HttpResponseMessage> RegisterAsync(RegisterMemberRequest request)
        => await http.PostAsJsonAsync("api/v1/members/register", request);

    public async Task<HttpResponseMessage> UpdateProfileAsync(UpdateProfileRequest request)
        => await http.PutAsJsonAsync("api/v1/members/me", request);

    public async Task<HttpResponseMessage> ApproveAsync(Guid id)
        => await http.PostAsync($"api/v1/members/{id}/approve", null);

    public async Task<HttpResponseMessage> RejectAsync(Guid id, RejectMemberRequest request)
        => await http.PostAsJsonAsync($"api/v1/members/{id}/reject", request);

    public async Task<HttpResponseMessage> SuspendAsync(Guid id, SuspendMemberRequest request)
        => await http.PostAsJsonAsync($"api/v1/members/{id}/suspend", request);

    public async Task<HttpResponseMessage> ReinstateAsync(Guid id)
        => await http.PostAsync($"api/v1/members/{id}/reinstate", null);

    public async Task<HttpResponseMessage> AssignRoleAsync(Guid id, AssignRoleRequest request)
        => await http.PostAsJsonAsync($"api/v1/members/{id}/roles", request);

    public async Task<HttpResponseMessage> RemoveRoleAsync(Guid id, string role)
        => await http.DeleteAsync($"api/v1/members/{id}/roles/{role}");
}

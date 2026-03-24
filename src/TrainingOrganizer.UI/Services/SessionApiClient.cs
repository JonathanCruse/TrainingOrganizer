using System.Net.Http.Json;
using TrainingOrganizer.Shared.Models;

namespace TrainingOrganizer.UI.Services;

public sealed class SessionApiClient(HttpClient http)
{
    public async Task<PagedResponse<TrainingSessionResponse>?> GetAllAsync(
        int page = 1, int pageSize = 20, Guid? recurringTrainingId = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        var url = $"api/v1/sessions?page={page}&pageSize={pageSize}";
        if (recurringTrainingId.HasValue)
            url += $"&recurringTrainingId={recurringTrainingId.Value}";
        if (from.HasValue)
            url += $"&from={from.Value:O}";
        if (to.HasValue)
            url += $"&to={to.Value:O}";
        return await http.GetFromJsonAsync<PagedResponse<TrainingSessionResponse>>(url);
    }

    public async Task<TrainingSessionResponse?> GetByIdAsync(Guid id)
        => await http.GetFromJsonAsync<TrainingSessionResponse>($"api/v1/sessions/{id}");

    public async Task<HttpResponseMessage> JoinAsync(Guid id)
        => await http.PostAsync($"api/v1/sessions/{id}/participants", null);

    public async Task<HttpResponseMessage> LeaveAsync(Guid id)
        => await http.DeleteAsync($"api/v1/sessions/{id}/participants/me");

    public async Task<HttpResponseMessage> AcceptParticipantAsync(Guid sessionId, Guid memberId)
        => await http.PostAsync($"api/v1/sessions/{sessionId}/participants/{memberId}/accept", null);

    public async Task<HttpResponseMessage> RejectParticipantAsync(Guid sessionId, Guid memberId)
        => await http.PostAsync($"api/v1/sessions/{sessionId}/participants/{memberId}/reject", null);

    public async Task<HttpResponseMessage> CancelAsync(Guid id, CancelSessionRequest request)
        => await http.PostAsJsonAsync($"api/v1/sessions/{id}/cancel", request);

    public async Task<HttpResponseMessage> CompleteAsync(Guid id)
        => await http.PostAsync($"api/v1/sessions/{id}/complete", null);

    public async Task<HttpResponseMessage> RecordAttendanceAsync(Guid id, RecordSessionAttendanceRequest request)
        => await http.PostAsJsonAsync($"api/v1/sessions/{id}/attendance", request);
}

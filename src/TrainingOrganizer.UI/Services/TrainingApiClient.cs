using System.Net.Http.Json;
using TrainingOrganizer.Shared.Models;

namespace TrainingOrganizer.UI.Services;

public sealed class TrainingApiClient(HttpClient http)
{
    public async Task<PagedResponse<TrainingResponse>?> GetAllAsync(
        int page = 1, int pageSize = 20, string? status = null, string? search = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        var url = $"api/v1/trainings?page={page}&pageSize={pageSize}";
        if (status is not null)
            url += $"&status={Uri.EscapeDataString(status)}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (from.HasValue)
            url += $"&from={from.Value:O}";
        if (to.HasValue)
            url += $"&to={to.Value:O}";
        return await http.GetFromJsonAsync<PagedResponse<TrainingResponse>>(url);
    }

    public async Task<TrainingResponse?> GetByIdAsync(Guid id)
        => await http.GetFromJsonAsync<TrainingResponse>($"api/v1/trainings/{id}");

    public async Task<HttpResponseMessage> CreateAsync(CreateTrainingRequest request)
        => await http.PostAsJsonAsync("api/v1/trainings", request);

    public async Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateTrainingRequest request)
        => await http.PutAsJsonAsync($"api/v1/trainings/{id}", request);

    public async Task<HttpResponseMessage> PublishAsync(Guid id)
        => await http.PostAsync($"api/v1/trainings/{id}/publish", null);

    public async Task<HttpResponseMessage> CancelAsync(Guid id, CancelTrainingRequest request)
        => await http.PostAsJsonAsync($"api/v1/trainings/{id}/cancel", request);

    public async Task<HttpResponseMessage> CompleteAsync(Guid id)
        => await http.PostAsync($"api/v1/trainings/{id}/complete", null);

    public async Task<HttpResponseMessage> JoinAsync(Guid id)
        => await http.PostAsync($"api/v1/trainings/{id}/participants", null);

    public async Task<HttpResponseMessage> LeaveAsync(Guid id)
        => await http.DeleteAsync($"api/v1/trainings/{id}/participants/me");

    public async Task<HttpResponseMessage> AcceptParticipantAsync(Guid trainingId, Guid memberId)
        => await http.PostAsync($"api/v1/trainings/{trainingId}/participants/{memberId}/accept", null);

    public async Task<HttpResponseMessage> RejectParticipantAsync(Guid trainingId, Guid memberId)
        => await http.PostAsync($"api/v1/trainings/{trainingId}/participants/{memberId}/reject", null);

    public async Task<HttpResponseMessage> RecordAttendanceAsync(Guid id, RecordAttendanceRequest request)
        => await http.PostAsJsonAsync($"api/v1/trainings/{id}/attendance", request);

    public async Task<HttpResponseMessage> AssignTrainerAsync(Guid trainingId, Guid trainerId)
        => await http.PostAsJsonAsync($"api/v1/trainings/{trainingId}/trainers", new AssignTrainerRequest(trainerId));

    public async Task<HttpResponseMessage> RemoveTrainerAsync(Guid trainingId, Guid trainerId)
        => await http.DeleteAsync($"api/v1/trainings/{trainingId}/trainers/{trainerId}");
}

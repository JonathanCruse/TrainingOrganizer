using System.Net.Http.Json;
using TrainingOrganizer.Shared.Models;

namespace TrainingOrganizer.UI.Services;

public sealed class RecurringTrainingApiClient(HttpClient http)
{
    public async Task<PagedResponse<RecurringTrainingResponse>?> GetAllAsync(int page = 1, int pageSize = 20)
        => await http.GetFromJsonAsync<PagedResponse<RecurringTrainingResponse>>($"api/v1/recurring-trainings?page={page}&pageSize={pageSize}");

    public async Task<RecurringTrainingResponse?> GetByIdAsync(Guid id)
        => await http.GetFromJsonAsync<RecurringTrainingResponse>($"api/v1/recurring-trainings/{id}");

    public async Task<HttpResponseMessage> CreateAsync(CreateRecurringTrainingRequest request)
        => await http.PostAsJsonAsync("api/v1/recurring-trainings", request);

    public async Task<HttpResponseMessage> PauseAsync(Guid id)
        => await http.PostAsync($"api/v1/recurring-trainings/{id}/pause", null);

    public async Task<HttpResponseMessage> ResumeAsync(Guid id)
        => await http.PostAsync($"api/v1/recurring-trainings/{id}/resume", null);

    public async Task<HttpResponseMessage> EndAsync(Guid id)
        => await http.PostAsync($"api/v1/recurring-trainings/{id}/end", null);

    public async Task<HttpResponseMessage> GenerateSessionsAsync(Guid id, GenerateSessionsRequest request)
        => await http.PostAsJsonAsync($"api/v1/recurring-trainings/{id}/generate", request);
}

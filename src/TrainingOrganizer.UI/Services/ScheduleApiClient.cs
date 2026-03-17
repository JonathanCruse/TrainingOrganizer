using System.Net.Http.Json;
using TrainingOrganizer.Shared.Models;

namespace TrainingOrganizer.UI.Services;

public sealed class ScheduleApiClient(HttpClient http)
{
    public async Task<List<ScheduleEntryResponse>?> GetMyScheduleAsync(DateTimeOffset from, DateTimeOffset to)
        => await http.GetFromJsonAsync<List<ScheduleEntryResponse>>(
            $"api/v1/schedule/me?from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}");

    public async Task<List<ScheduleEntryResponse>?> GetTrainerScheduleAsync(Guid trainerId, DateTimeOffset from, DateTimeOffset to)
        => await http.GetFromJsonAsync<List<ScheduleEntryResponse>>(
            $"api/v1/schedule/trainers/{trainerId}?from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}");
}

using System.Net.Http.Json;
using TrainingOrganizer.Shared.Models;

namespace TrainingOrganizer.UI.Services;

public sealed class FacilityApiClient(HttpClient http)
{
    // Locations
    public async Task<PagedResponse<LocationResponse>?> GetAllLocationsAsync(int page = 1, int pageSize = 20)
        => await http.GetFromJsonAsync<PagedResponse<LocationResponse>>($"api/v1/locations?page={page}&pageSize={pageSize}");

    public async Task<LocationResponse?> GetLocationByIdAsync(Guid id)
        => await http.GetFromJsonAsync<LocationResponse>($"api/v1/locations/{id}");

    public async Task<HttpResponseMessage> CreateLocationAsync(CreateLocationRequest request)
        => await http.PostAsJsonAsync("api/v1/locations", request);

    public async Task<HttpResponseMessage> UpdateLocationAsync(Guid id, UpdateLocationRequest request)
        => await http.PutAsJsonAsync($"api/v1/locations/{id}", request);

    public async Task<HttpResponseMessage> AddRoomAsync(Guid locationId, AddRoomRequest request)
        => await http.PostAsJsonAsync($"api/v1/locations/{locationId}/rooms", request);

    public async Task<HttpResponseMessage> UpdateRoomAsync(Guid locationId, Guid roomId, UpdateRoomRequest request)
        => await http.PutAsJsonAsync($"api/v1/locations/{locationId}/rooms/{roomId}", request);

    // Bookings
    public async Task<PagedResponse<BookingResponse>?> GetBookingsAsync(int page = 1, int pageSize = 20)
        => await http.GetFromJsonAsync<PagedResponse<BookingResponse>>($"api/v1/bookings?page={page}&pageSize={pageSize}");

    public async Task<HttpResponseMessage> CreateBookingAsync(CreateBookingRequest request)
        => await http.PostAsJsonAsync("api/v1/bookings", request);

    public async Task<HttpResponseMessage> RescheduleBookingAsync(Guid id, RescheduleBookingRequest request)
        => await http.PutAsJsonAsync($"api/v1/bookings/{id}/reschedule", request);

    public async Task<HttpResponseMessage> CancelBookingAsync(Guid id)
        => await http.PostAsync($"api/v1/bookings/{id}/cancel", null);
}

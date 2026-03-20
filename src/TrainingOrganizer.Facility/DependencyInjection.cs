using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Facility.Application.Repositories;
using TrainingOrganizer.Facility.Domain.Services;
using TrainingOrganizer.Facility.Infrastructure.Persistence.Repositories;
using TrainingOrganizer.Facility.Infrastructure.Services;

namespace TrainingOrganizer.Facility;

public static class DependencyInjection
{
    public static IServiceCollection AddFacility(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        // Domain services
        services.AddScoped<IRoomBookingService, RoomBookingService>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace TrainingOrganizer.UI.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        services.AddScoped<MemberApiClient>();
        services.AddScoped<TrainingApiClient>();
        services.AddScoped<RecurringTrainingApiClient>();
        services.AddScoped<FacilityApiClient>();
        services.AddScoped<ScheduleApiClient>();
        return services;
    }
}

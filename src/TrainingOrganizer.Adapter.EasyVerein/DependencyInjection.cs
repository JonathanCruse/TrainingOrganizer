using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Membership.Application.Services;

namespace TrainingOrganizer.Adapter.EasyVerein;

public static class DependencyInjection
{
    public static IServiceCollection AddEasyVereinAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EasyVereinSettings>(configuration.GetSection(EasyVereinSettings.SectionName));
        services.AddHttpClient<IEasyVereinApiClient, EasyVereinApiClient>();
        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Membership.Application.Services;

namespace TrainingOrganizer.Adapter.Keycloak;

public static class DependencyInjection
{
    public static IServiceCollection AddKeycloakAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakAdminSettings>(configuration.GetSection(KeycloakAdminSettings.SectionName));
        services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>();
        return services;
    }
}

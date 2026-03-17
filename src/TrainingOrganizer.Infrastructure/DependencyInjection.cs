using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Facility.Repositories;
using TrainingOrganizer.Application.Membership.Repositories;
using TrainingOrganizer.Application.Membership.Services;
using TrainingOrganizer.Application.Training.Repositories;
using TrainingOrganizer.Domain.Services;
using TrainingOrganizer.Infrastructure.ExternalServices.EasyVerein;
using TrainingOrganizer.Infrastructure.ExternalServices.Keycloak;
using TrainingOrganizer.Infrastructure.Persistence;
using TrainingOrganizer.Infrastructure.Persistence.Repositories;
using TrainingOrganizer.Infrastructure.Persistence.Serialization;
using TrainingOrganizer.Infrastructure.Services;

namespace TrainingOrganizer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.AddSingleton<MongoDbContext>();

        // Register BSON class maps
        MongoDbClassMaps.RegisterAll();

        // Repositories
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ITrainingRepository, TrainingRepository>();
        services.AddScoped<IRecurringTrainingRepository, RecurringTrainingRepository>();
        services.AddScoped<ITrainingSessionRepository, TrainingSessionRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        // Domain services
        services.AddScoped<IRoomBookingService, RoomBookingService>();
        services.AddScoped<ISessionGenerationService, SessionGenerationService>();
        services.AddScoped<IMemberUniquenessService, MemberUniquenessService>();

        // Application services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // External services — EasyVerein
        services.Configure<EasyVereinSettings>(configuration.GetSection(EasyVereinSettings.SectionName));
        services.AddHttpClient<IEasyVereinApiClient, EasyVereinApiClient>();

        // External services — Keycloak Admin
        services.Configure<KeycloakAdminSettings>(configuration.GetSection(KeycloakAdminSettings.SectionName));
        services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>();

        return services;
    }
}

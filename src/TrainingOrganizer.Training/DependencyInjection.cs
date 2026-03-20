using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Training.Application.Repositories;
using TrainingOrganizer.Training.Domain.Services;
using TrainingOrganizer.Training.Infrastructure.Persistence.Repositories;
using TrainingOrganizer.Training.Infrastructure.Services;

namespace TrainingOrganizer.Training;

public static class DependencyInjection
{
    public static IServiceCollection AddTraining(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ITrainingRepository, TrainingRepository>();
        services.AddScoped<IRecurringTrainingRepository, RecurringTrainingRepository>();
        services.AddScoped<ITrainingSessionRepository, TrainingSessionRepository>();

        // Domain services
        services.AddScoped<ISessionGenerationService, SessionGenerationService>();

        return services;
    }
}

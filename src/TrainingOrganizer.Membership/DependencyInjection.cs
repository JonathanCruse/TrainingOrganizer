using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.Membership.Domain.Services;
using TrainingOrganizer.Membership.Infrastructure.Persistence.Repositories;
using TrainingOrganizer.Membership.Infrastructure.Services;
using TrainingOrganizer.SharedKernel.Application.Interfaces;

namespace TrainingOrganizer.Membership;

public static class DependencyInjection
{
    public static IServiceCollection AddMembership(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IMemberRepository, MemberRepository>();

        // Domain services
        services.AddScoped<IMemberUniquenessService, MemberUniquenessService>();

        // Application services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}

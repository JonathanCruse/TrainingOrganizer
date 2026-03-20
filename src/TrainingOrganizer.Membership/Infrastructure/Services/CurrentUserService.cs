using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TrainingOrganizer.SharedKernel.Application.Interfaces;

namespace TrainingOrganizer.Membership.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? MemberId
    {
        get
        {
            var memberIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("member_id")?.Value;

            if (memberIdClaim is not null && Guid.TryParse(memberIdClaim, out var guid))
                return guid;

            return null;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?.User
            .IsInRole("Admin") ?? false;

    public bool IsTrainer =>
        _httpContextAccessor.HttpContext?.User
            .IsInRole("Trainer") ?? false;

    public bool IsGuest =>
        _httpContextAccessor.HttpContext?.User
            .IsInRole("Guest") ?? false;
}

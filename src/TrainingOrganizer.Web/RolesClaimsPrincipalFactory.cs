using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace TrainingOrganizer.Web;

public sealed class RolesClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    public RolesClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor)
        : base(accessor)
    {
    }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account, RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);

        if (user.Identity is not ClaimsIdentity identity)
            return user;

        // Parse JSON array role claims into individual claims
        var roleClaimType = options.RoleClaim ?? "roles";
        var roleClaims = identity.FindAll(roleClaimType).ToList();
        foreach (var roleClaim in roleClaims)
        {
            if (!roleClaim.Value.TrimStart().StartsWith("["))
                continue;

            identity.RemoveClaim(roleClaim);

            var roles = JsonSerializer.Deserialize<string[]>(roleClaim.Value);
            if (roles is null)
                continue;

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(roleClaimType, role));
            }
        }

        return user;
    }
}

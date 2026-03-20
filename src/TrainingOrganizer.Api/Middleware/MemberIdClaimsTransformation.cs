using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using TrainingOrganizer.Membership.Application.Repositories;

namespace TrainingOrganizer.Api.Middleware;

public sealed class MemberIdClaimsTransformation : IClaimsTransformation
{
    private readonly IMemberRepository _memberRepository;

    public MemberIdClaimsTransformation(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return principal;

        if (identity.HasClaim(c => c.Type == "member_id"))
            return principal;

        var sub = identity.FindFirst("sub")?.Value;
        if (sub is null)
            return principal;

        var member = await _memberRepository.GetByExternalIdentityAsync("keycloak", sub);
        if (member is not null)
        {
            identity.AddClaim(new Claim("member_id", member.Id.Value.ToString()));
        }

        return principal;
    }
}

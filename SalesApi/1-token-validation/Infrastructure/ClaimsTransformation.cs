using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace SalesApi.Infrastructure;

internal class ClaimsTransformation(IUserPermissionRepository userPermissionRepository) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        await Task.CompletedTask;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var identity = new ClaimsIdentity(principal.Identity);

            AddPermissionIfScope(identity, "products.read", new Claim("urn:permissions:products:read", "true"));
            AddPermissionIfScope(identity, "products.write", new Claim("urn:permissions:products:write", "true"));

            var userId = principal.FindFirstValue("sub");

            if (userId == null)
            {
                return new ClaimsPrincipal(identity);
            }

            var marketPermissions = await userPermissionRepository.GetUserMarketPermissions(userId);

            foreach (var marketPermission in marketPermissions)
            {
                identity.AddClaim(new Claim("urn:permissions:market", marketPermission));
            }

            return new ClaimsPrincipal(identity);
        }

        return principal;
    }

    private void AddPermissionIfScope(ClaimsIdentity identity, string scope, Claim claim)
    {
        if (identity.Claims.Any(claim => claim.Type == "scope" && claim.Value.Split(' ').Contains(scope)))
        {
            identity.AddClaim(claim);
        }
        else if (identity.Claims.Any(c => c.Type == "scope" && c.Value == scope))
        {
            identity.AddClaim(claim);
        }
    }
}
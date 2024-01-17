using System.Security.Claims;

namespace SalesApi.Infrastructure;

public interface IPermissionService
{
    bool CanReadProducts { get; }

    bool CanWriteProducts { get; }

    string MarketId { get; }

    string? UserId { get; }

    bool HasPermissionToMarket(string requestedMarket);
}

public class HttpContextPermissionService : IPermissionService
{
    public HttpContextPermissionService(IHttpContextAccessor contextAccessor, IUserPermissionRepository userPermissionRepository)
    {
        var principal = contextAccessor.HttpContext?.User;

        if (principal == null)
        {
            if (contextAccessor.HttpContext == null)
            {
                throw new ArgumentException("HTTP Context is null", nameof(contextAccessor));
            }

            throw new ArgumentException("User object is null", nameof(contextAccessor));
        }

        UserId = principal.FindFirstValue("sub");

        // It is important to honor any scope that affect our domain
        IfScope(principal, "products.read", () => CanReadProducts = true);
        IfScope(principal, "products.write", () => CanWriteProducts = true);

        if (UserId is not null)
        {
            var userPermissions = userPermissionRepository.GetUserMarketPermissions(UserId).GetAwaiter().GetResult();
            MarketId = userPermissions.FirstOrDefault();
        }
    }

    public bool CanReadProducts { get; private set; }

    public bool CanWriteProducts { get; private set; }

    public string? MarketId { get; private set; }

    public string? UserId { get; private set; }

    public string? ClientId { get; private set; }

    public bool HasPermissionToMarket(string requestedMarket)
    {
        if (MarketId is null) return false;
        return string.Equals(MarketId, requestedMarket, StringComparison.OrdinalIgnoreCase);
    }

    private static void IfScope(ClaimsPrincipal principal, string scope, Action action)
    {
        //Scopes can be a space separated list of scopes, so we need to split and check each scope
        if (principal.Claims.Any(claim => claim.Type == "scope" && claim.Value.Split(' ').Contains(scope)))
        {
            action();
        }

        if (principal.HasClaim(claim => claim.Type == "scope" && claim.Value == scope))
        {
            action();
        }
    }
}
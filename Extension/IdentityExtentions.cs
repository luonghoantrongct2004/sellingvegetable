using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace FruityFresh.Extension
{
    public static class IdentityExtentions
    {
        public static string GetAccountId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("AdminId");
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetUsername(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Username");
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetImage(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Image");
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetSpecificClaim(this ClaimsPrincipal principal, string claimType)
        {
            var claims = principal.Claims.FirstOrDefault(x => x.Type == claimType);
            return (claims != null) ? claims.Value :  string.Empty;
        }
    }
}

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Template_API_Core2_1.Models;

namespace Template_API_Core2_1.Interfaces
{
    public interface ITokenService
    {
        Task<object> GetClaimsIdentityAsync(Actor actor, CancellationToken cancellationToken = default(CancellationToken));
        object RenewClaimsIdentity(ClaimsPrincipal claims);
    }
}

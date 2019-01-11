using System.Threading;
using System.Threading.Tasks;
using Template_API_Core2_1.Models;

namespace Template_API_Core2_1.Interfaces
{
    public interface ITokenRepository
    {
        Task<Authentication> GetClaimValuesAsync(Actor actor, CancellationToken cancellationToken = default(CancellationToken));
    }
}

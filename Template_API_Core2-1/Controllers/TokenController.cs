using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Template_API_Core2_1.Interfaces;
using Template_API_Core2_1.Models;
using Template_API_Core2_1.Options;

namespace Template_API_Core2_1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService tokenService;
        private readonly JwtIssuerOptions jwtOptions;

        /// <summary>
        /// Constructor, using Dependancy Injection to apply the service and Token options
        /// Both items are guarded to ensure that they are not passed null values
        /// </summary>
        /// <param name="service">The service for tokens</param>
        /// <param name="jwtIssuerOptions">The Token options to use for configuration</param>
        public TokenController(ITokenService service, IOptions<JwtIssuerOptions> jwtIssuerOptions)
        {
            tokenService = service ?? throw new ArgumentNullException(nameof(service));
            jwtOptions = jwtIssuerOptions.Value ?? throw new ArgumentNullException(nameof(jwtIssuerOptions));
        }

        /// <summary>
        /// Handle a new login attempt against the API, the body must contain
        /// the user details for gaining access to the system
        /// </summary>
        /// <param name="actor">The username/password model for validating the user login</param>
        /// <returns>The authenticated and signed token</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Actor actor)
        {
            object token = await tokenService.GetClaimsIdentityAsync(actor);

            if (null == token)
            {
                return Unauthorized();
            }

            return Ok(token);
        }

        /// <summary>
        /// Renews a valid token extending it's expire date time value
        /// NOTE: this must be an authenticated end point so that the
        /// original token is validated to prevent incorrect access
        /// </summary>
        /// <returns>The authenticated and signed token with a new expiry date time</returns>
        [HttpPost]
        public IActionResult Post()
        {
            object token = tokenService.RenewClaimsIdentity(User);

            if (null == token)
            {
                return Unauthorized();
            }

            return Ok(token);
        }
    }
}
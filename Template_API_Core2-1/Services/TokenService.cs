using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Template_API_Core2_1.Interfaces;
using Template_API_Core2_1.Models;
using Template_API_Core2_1.Options;

namespace Template_API_Core2_1.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository tokenRepository;
        private readonly JwtIssuerOptions jwtOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository">The repository contract to use</param>
        /// <param name="jwtIssuerOptions">The JWT values/settings</param>
        public TokenService(ITokenRepository repository, IOptions<JwtIssuerOptions> jwtIssuerOptions)
        {
            tokenRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            jwtOptions = jwtIssuerOptions.Value ?? throw new ArgumentNullException(nameof(jwtIssuerOptions));
        }

        /// <summary>
        /// Validates the Actor details against the supplied Repository and then 
        /// builds the Claims and finally produces the encrypted JWT token to
        /// respond with
        /// </summary>
        /// <param name="actor">The Actor model to use for authentication</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The object containing the Token value the number of seconds until it expires</returns>
        public async Task<object> GetClaimsIdentityAsync(Actor actor, CancellationToken cancellationToken = default(CancellationToken))
        {
            object response = null;

            // Get the authentication from the database, it should return null in the event of a failure
            Authentication auth = await tokenRepository.GetClaimValuesAsync(actor, cancellationToken);

            try
            {
                // Ensure we have an authenticated model from the repository
                if (null == auth)
                {
                    // Get the generated token and set it as the response
                    string encodedJwt = GenerateEncodedToken(auth);
                    response = new { access_token = encodedJwt, expires_in = (int)jwtOptions.ValidFor.TotalSeconds };
                }
            }
            catch (ArgumentException argEx)
            {
                // Reset the response to null to ensure Authentication Failure occurs
                response = null;

                // Output some error details, if we're in debug include the stacktrace
                Trace.WriteLine(argEx.Message);
#if DEBUG
                Trace.WriteLine(argEx.StackTrace);
#endif
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenCompressionFailedException tokenFailedEx)
            {
                // The token may have been valid, but as this is security void it
                response = null;

                // Output some error details, if we're in debug include the stacktrace
                Trace.WriteLine(tokenFailedEx.Message);
#if DEBUG
                Trace.WriteLine(tokenFailedEx.StackTrace);
#endif
            }

            // Return the 
            return response;
        }

        /// <summary>
        /// Only call from Authenticated end points!
        /// 
        /// Provides a new token to replace an old or expiring one (but not an invalid token)
        /// </summary>
        /// <returns></returns>
        public object RenewClaimsIdentity(ClaimsPrincipal claims)
        {
            Authentication auth = new Authentication
            {
                EmailAddress = claims.FindFirst(ClaimTypes.Email)?.Value,
                PrimaryId = Convert.ToInt32(claims.FindFirst(ClaimTypes.PrimarySid)?.Value),
                PrimaryGroupId = Convert.ToInt32(claims.FindFirst(ClaimTypes.PrimaryGroupSid)?.Value),
                RoleId = Convert.ToInt32(claims.FindFirst(ClaimTypes.Role)?.Value),
            };

            // Geneate and assign the new token values
            string encodedToken = GenerateEncodedToken(auth);
            object response = new { access_token = encodedToken, expires_in = (int)jwtOptions.ValidFor.TotalSeconds };

            // Return the new token object
            return response;
        }

        /// <summary>
        /// Generates the token from a populated Authentication model
        /// </summary>
        /// <param name="identity">The Identity to use for generating the token</param>
        /// <returns>The generated and encrtpyed token</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Microsoft.IdentityModel.Tokens.SecurityTokenEncryptionFailedException"></exception>
        private string GenerateEncodedToken(Authentication auth)
        {
            string response = string.Empty;

            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(auth.EmailAddress, "Token"), new[]
                {
                    new Claim(ClaimTypes.Email, auth.EmailAddress),
                    new Claim(ClaimTypes.PrimarySid, auth.PrimaryId.ToString()),
                    new Claim(ClaimTypes.PrimaryGroupSid, auth.PrimaryGroupId.ToString()),
                    new Claim(ClaimTypes.Role, auth.RoleId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtOptions.JtiGenerator().Result),
                    new Claim(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)jwtOptions.IssuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                }
            );

            // If the claim is populated we can build the token
            if (null != identity)
            {
                // The token
                JwtSecurityToken jwt = new JwtSecurityToken(
                    issuer: jwtOptions.Issuer,
                    audience: jwtOptions.Audience,
                    claims: identity.Claims,
                    notBefore: jwtOptions.NotBefore,
                    expires: jwtOptions.Expiration,
                    signingCredentials: jwtOptions.SigningCredentials);

                // The encoded token values
                response = new JwtSecurityTokenHandler().WriteToken(jwt);
            }

            return response;
        }
    }
}

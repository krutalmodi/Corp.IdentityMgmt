using Corp.IdentityMgmt.Application.Dtos;
using Corp.IdentityMgmt.Application.DTOs;
using Corp.IdentityMgmt.Application.Handlers;
using Corp.IdentityMgmt.Application.Interfaces;
using Corp.IdentityMgmt.Infrastructure.ExternalProviders;
using Microsoft.AspNetCore.Mvc;

namespace Corp.IdentityMgmt.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly GoogleOAuthProvider? _googleProvider;
        private readonly GitHubOAuthProvider? _githubProvider;
        private readonly MicrosoftOAuthProvider? _microsoftProvider;

        public AuthController(
            IUserRepository userRepository, 
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            GoogleOAuthProvider? googleProvider = null,
            GitHubOAuthProvider? githubProvider = null,
            MicrosoftOAuthProvider? microsoftProvider = null)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _googleProvider = googleProvider;
            _githubProvider = githubProvider;
            _microsoftProvider = microsoftProvider;
        }

        /// <summary>
        /// Registers a new user with the provided registration information.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                Guid tenantId = ResolveTenantId();
                var handler = new RegisterUserHandler(_userRepository, _passwordHasher);
                await handler.RegisterAsync(dto, tenantId);
                return Ok(new { message = "User registered successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Authenticates a user based on the provided credentials and returns a token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                Guid tenantId = ResolveTenantId();
                var handler = new LoginHandler(_userRepository, _passwordHasher, _tokenService);
                var token = await handler.LoginAsync(dto, tenantId);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Initiates external login by returning the OAuth provider's authorization URL.
        /// </summary>
        /// <param name="request">The external login request containing provider name and redirect URI.</param>
        /// <returns>Authorization URL to redirect the user to the OAuth provider.</returns>
        [HttpPost("external-login")]
        public IActionResult InitiateExternalLogin([FromBody] ExternalLoginRequestDto request)
        {
            try
            {
                var state = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
                string? authUrl = null;

                return request.ProviderName?.ToLower() switch
                {
                    "google" when _googleProvider != null => 
                        Ok(new ExternalLoginUrlResponseDto
                        {
                            AuthorizationUrl = _googleProvider.GenerateAuthorizationUrl(request.RedirectUri, state),
                            ProviderName = "Google",
                            State = state
                        }),

                    "github" when _githubProvider != null => 
                        Ok(new ExternalLoginUrlResponseDto
                        {
                            AuthorizationUrl = _githubProvider.GenerateAuthorizationUrl(request.RedirectUri, state),
                            ProviderName = "GitHub",
                            State = state
                        }),

                    "microsoft" when _microsoftProvider != null => 
                        Ok(new ExternalLoginUrlResponseDto
                        {
                            AuthorizationUrl = _microsoftProvider.GenerateAuthorizationUrl(request.RedirectUri, state),
                            ProviderName = "Microsoft",
                            State = state
                        }),

                    _ => BadRequest(new { error = "Unsupported or unconfigured provider" })
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Handles OAuth callback and returns JWT token if authentication is successful.
        /// </summary>
        /// <param name="provider">The OAuth provider name (google, github, microsoft).</param>
        /// <param name="code">The authorization code from the OAuth provider.</param>
        /// <param name="state">The state parameter for CSRF validation.</param>
        /// <returns>JWT token and user information if successful.</returns>
        [HttpGet("callback/{provider}")]
        public async Task<IActionResult> ExternalLoginCallback(
            string provider,
            [FromQuery] string code,
            [FromQuery] string state)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return BadRequest(new { error = "Authorization code is missing" });
                }

                Guid tenantId = ResolveTenantId();
                var redirectUri = _configuration[$"OAuth:{provider}:RedirectUri"] ?? 
                                 $"https://localhost:44388/api/auth/callback/{provider}";

                OAuthProviderBase? providerInstance = provider.ToLower() switch
                {
                    "google" => _googleProvider,
                    "github" => _githubProvider,
                    "microsoft" => _microsoftProvider,
                    _ => null
                };

                if (providerInstance == null)
                {
                    return BadRequest(new { error = "Unsupported or unconfigured provider" });
                }

                // Get user info from OAuth provider
                var externalUserInfo = await providerInstance.GetUserInfoAsync(code, redirectUri);

                if (externalUserInfo == null)
                {
                    return BadRequest(new { error = "Failed to retrieve user information from provider" });
                }

                // Handle the external login
                var handler = new ExternalAuthHandler(_userRepository, _tokenService);
                var result = await handler.HandleExternalLoginAsync(externalUserInfo, tenantId, provider);

                if (result.Token == null)
                {
                    return BadRequest(new { error = result.Message });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"External login failed: {ex.Message}" });
            }
        }

        /// <summary>
        /// Gets the list of available external login providers.
        /// </summary>
        [HttpGet("external-providers")]
        public IActionResult GetExternalProviders()
        {
            var providers = new List<string>();

            if (_googleProvider != null)
                providers.Add("Google");

            if (_githubProvider != null)
                providers.Add("GitHub");

            if (_microsoftProvider != null)
                providers.Add("Microsoft");

            return Ok(new { providers });
        }

        /// <summary>
        /// Resolves the tenant identifier associated with the current request context.
        /// </summary>
        private Guid ResolveTenantId()
        {
            // Phase-1 : For demonstration purposes, we return a fixed tenant ID. 
            // In a real application, this method would contain logic to determine the tenant ID 
            // based on the request context, such as examining the domain, headers, or authentication information.
            return Guid.Parse("11111111-1111-1111-1111-111111111111");
        }
    }
}

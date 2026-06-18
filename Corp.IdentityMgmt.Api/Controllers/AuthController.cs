using Corp.IdentityMgmt.Application.DTOs;
using Corp.IdentityMgmt.Application.Handlers;
using Corp.IdentityMgmt.Application.Interfaces;
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

        public AuthController(IUserRepository userRepository, 
            ITokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Registers a new user with the provided registration information.
        /// </summary>
        /// <remarks>The tenant ID is resolved before attempting to register the user. If an invalid
        /// operation occurs during registration, a bad request response is returned with the relevant error
        /// message.</remarks>
        /// <param name="dto">The registration data transfer object containing the user's registration details, such as username and
        /// password. This parameter is required.</param>
        /// <returns>An IActionResult that indicates the result of the registration operation. Returns a success message if
        /// registration is successful; otherwise, returns a bad request with an error message.</returns>
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
        /// Authenticates a user based on the provided credentials and returns a token if authentication is successful.
        /// </summary>
        /// <remarks>This method resolves the tenant context and delegates authentication to a login
        /// handler. An unauthorized response is returned if the credentials are invalid.</remarks>
        /// <param name="dto">The login data transfer object containing the user's credentials to be validated.</param>
        /// <returns>An <see cref="IActionResult"/> that contains the generated authentication token if the login is successful;
        /// otherwise, an unauthorized response.</returns>
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
        /// Resolves the tenant identifier associated with the current request context.
        /// </summary>
        /// <remarks>In a production environment, this method would typically determine the tenant ID
        /// based on contextual information such as the request's domain, headers, or authentication data. In this
        /// example, a fixed tenant ID is returned for demonstration purposes.</remarks>
        /// <returns>A <see cref="System.Guid"/> representing the tenant ID for the current context.</returns>
        private Guid ResolveTenantId()
        {
            // Phase-1 : For demonstration purposes, we return a fixed tenant ID. In a real application, this method would contain logic to determine the tenant ID based on the request context, such as examining the domain, headers, or authentication information.
            // For this example, we'll just return a fixed tenant ID for demonstration purposes.
            return Guid.Parse("11111111-1111-1111-1111-111111111111");
        }


    }
}

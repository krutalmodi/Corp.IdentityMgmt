namespace Corp.IdentityMgmt.Application.Dtos
{
    /// <summary>
    /// Represents a request to initiate an external login flow.
    /// </summary>
    public class ExternalLoginRequestDto
    {
        /// <summary>
        /// The name of the external provider (e.g., "Google", "GitHub", "Microsoft").
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// The redirect URI where the OAuth provider will redirect after authentication.
        /// Must be pre-registered with the OAuth provider.
        /// </summary>
        public string RedirectUri { get; set; }
    }

    /// <summary>
    /// Represents the response with the authorization URL for external login.
    /// </summary>
    public class ExternalLoginUrlResponseDto
    {
        /// <summary>
        /// The OAuth authorization URL to redirect the user to.
        /// </summary>
        public string AuthorizationUrl { get; set; }

        /// <summary>
        /// The provider name for reference.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// State parameter used to prevent CSRF attacks.
        /// Should be stored and validated on callback.
        /// </summary>
        public string State { get; set; }
    }

    /// <summary>
    /// Represents a callback request after OAuth authentication.
    /// </summary>
    public class ExternalLoginCallbackDto
    {
        /// <summary>
        /// The authorization code returned by the OAuth provider.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The state parameter to validate CSRF protection.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The name of the external provider.
        /// </summary>
        public string ProviderName { get; set; }
    }

    /// <summary>
    /// Represents the response after successful external authentication.
    /// </summary>
    public class ExternalLoginResponseDto
    {
        /// <summary>
        /// JWT token for API authentication if user was created or linked.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Indicates if this is a new user registration via external provider.
        /// </summary>
        public bool IsNewUser { get; set; }

        /// <summary>
        /// Success message or error description.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// External provider user information for display.
        /// </summary>
        public ExternalUserInfoDto? UserInfo { get; set; }
    }

    /// <summary>
    /// Represents external provider user information.
    /// </summary>
    public class ExternalUserInfoDto
    {
        public string ProviderUserId { get; set; }
        public string Provider { get; set; }
        public string Email { get; set; }
        public string? Name { get; set; }
        public string? Picture { get; set; }
    }
}

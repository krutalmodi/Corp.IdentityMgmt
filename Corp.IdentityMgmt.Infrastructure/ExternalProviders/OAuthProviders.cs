using Corp.IdentityMgmt.Application.Dtos;
using System.Text.Json;

namespace Corp.IdentityMgmt.Infrastructure.ExternalProviders
{
    /// <summary>
    /// Abstract base class for OAuth providers.
    /// </summary>
    public abstract class OAuthProviderBase
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _clientId;
        protected readonly string _clientSecret;

        public string ProviderName { get; protected set; }

        protected OAuthProviderBase(HttpClient httpClient, string clientId, string clientSecret)
        {
            _httpClient = httpClient;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <summary>
        /// Generates the OAuth authorization URL for the provider.
        /// </summary>
        public abstract string GenerateAuthorizationUrl(string redirectUri, string state);

        /// <summary>
        /// Exchanges the authorization code for user information.
        /// </summary>
        public abstract Task<ExternalUserInfoDto?> GetUserInfoAsync(string code, string redirectUri);

        protected string GenerateState()
        {
            return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        }
    }

    /// <summary>
    /// Google OAuth provider implementation.
    /// </summary>
    public class GoogleOAuthProvider : OAuthProviderBase
    {
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

        public GoogleOAuthProvider(HttpClient httpClient, string clientId, string clientSecret)
            : base(httpClient, clientId, clientSecret)
        {
            ProviderName = "Google";
        }

        public override string GenerateAuthorizationUrl(string redirectUri, string state)
        {
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "redirect_uri", redirectUri },
                { "response_type", "code" },
                { "scope", "openid email profile" },
                { "state", state },
                { "access_type", "offline" }
            };

            var query = string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            return $"{AuthorizationEndpoint}?{query}";
        }

        public override async Task<ExternalUserInfoDto?> GetUserInfoAsync(string code, string redirectUri)
        {
            try
            {
                // Exchange code for access token
                var tokenRequest = new Dictionary<string, string>
                {
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "code", code },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", redirectUri }
                };

                var tokenResponse = await _httpClient.PostAsync(TokenEndpoint,
                    new FormUrlEncodedContent(tokenRequest));

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenJson = JsonSerializer.Deserialize<JsonElement>(tokenContent);
                var accessToken = tokenJson.GetProperty("access_token").GetString();

                // Get user info
                var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
                userInfoRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var userResponse = await _httpClient.SendAsync(userInfoRequest);
                if (!userResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var userJson = JsonSerializer.Deserialize<JsonElement>(userContent);

                return new ExternalUserInfoDto
                {
                    ProviderUserId = userJson.GetProperty("id").GetString() ?? "",
                    Provider = ProviderName,
                    Email = userJson.GetProperty("email").GetString() ?? "",
                    Name = userJson.TryGetProperty("name", out var name) ? name.GetString() : null,
                    Picture = userJson.TryGetProperty("picture", out var picture) ? picture.GetString() : null
                };
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// GitHub OAuth provider implementation.
    /// </summary>
    public class GitHubOAuthProvider : OAuthProviderBase
    {
        private const string AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        private const string TokenEndpoint = "https://github.com/login/oauth/access_token";
        private const string UserInfoEndpoint = "https://api.github.com/user";

        public GitHubOAuthProvider(HttpClient httpClient, string clientId, string clientSecret)
            : base(httpClient, clientId, clientSecret)
        {
            ProviderName = "GitHub";
        }

        public override string GenerateAuthorizationUrl(string redirectUri, string state)
        {
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "redirect_uri", redirectUri },
                { "scope", "user:email" },
                { "state", state }
            };

            var query = string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            return $"{AuthorizationEndpoint}?{query}";
        }

        public override async Task<ExternalUserInfoDto?> GetUserInfoAsync(string code, string redirectUri)
        {
            try
            {
                // Exchange code for access token
                var tokenRequest = new Dictionary<string, string>
                {
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "code", code },
                    { "redirect_uri", redirectUri }
                };

                var tokenResponse = await _httpClient.PostAsync(TokenEndpoint,
                    new FormUrlEncodedContent(tokenRequest));

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenJson = JsonSerializer.Deserialize<JsonElement>(tokenContent);
                var accessToken = tokenJson.GetProperty("access_token").GetString();

                // Get user info
                var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
                userInfoRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                userInfoRequest.Headers.Add("User-Agent", "Corp.IdentityMgmt");

                var userResponse = await _httpClient.SendAsync(userInfoRequest);
                if (!userResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var userJson = JsonSerializer.Deserialize<JsonElement>(userContent);

                var email = userJson.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;

                // If email is null, fetch from primary email endpoint
                if (string.IsNullOrWhiteSpace(email))
                {
                    email = await GetPrimaryEmailAsync(accessToken);
                }

                return new ExternalUserInfoDto
                {
                    ProviderUserId = userJson.GetProperty("id").GetInt32().ToString(),
                    Provider = ProviderName,
                    Email = email ?? "",
                    Name = userJson.TryGetProperty("name", out var name) ? name.GetString() : null,
                    Picture = userJson.TryGetProperty("avatar_url", out var avatar) ? avatar.GetString() : null
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<string?> GetPrimaryEmailAsync(string accessToken)
        {
            try
            {
                var emailRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
                emailRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                emailRequest.Headers.Add("User-Agent", "Corp.IdentityMgmt");

                var response = await _httpClient.SendAsync(emailRequest);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var emails = JsonSerializer.Deserialize<JsonElement[]>(content);

                var primaryEmail = emails?.FirstOrDefault(e => 
                    e.TryGetProperty("primary", out var primary) && primary.GetBoolean());

                return primaryEmail?.GetProperty("email").GetString();
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Microsoft OAuth provider implementation.
    /// </summary>
    public class MicrosoftOAuthProvider : OAuthProviderBase
    {
        private const string AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
        private const string TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
        private const string UserInfoEndpoint = "https://graph.microsoft.com/v1.0/me";

        public MicrosoftOAuthProvider(HttpClient httpClient, string clientId, string clientSecret)
            : base(httpClient, clientId, clientSecret)
        {
            ProviderName = "Microsoft";
        }

        public override string GenerateAuthorizationUrl(string redirectUri, string state)
        {
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "redirect_uri", redirectUri },
                { "response_type", "code" },
                { "scope", "openid profile email" },
                { "state", state },
                { "response_mode", "query" }
            };

            var query = string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            return $"{AuthorizationEndpoint}?{query}";
        }

        public override async Task<ExternalUserInfoDto?> GetUserInfoAsync(string code, string redirectUri)
        {
            try
            {
                // Exchange code for access token
                var tokenRequest = new Dictionary<string, string>
                {
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "code", code },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", redirectUri },
                    { "scope", "openid profile email" }
                };

                var tokenResponse = await _httpClient.PostAsync(TokenEndpoint,
                    new FormUrlEncodedContent(tokenRequest));

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenJson = JsonSerializer.Deserialize<JsonElement>(tokenContent);
                var accessToken = tokenJson.GetProperty("access_token").GetString();

                // Get user info
                var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
                userInfoRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var userResponse = await _httpClient.SendAsync(userInfoRequest);
                if (!userResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var userJson = JsonSerializer.Deserialize<JsonElement>(userContent);

                return new ExternalUserInfoDto
                {
                    ProviderUserId = userJson.GetProperty("id").GetString() ?? "",
                    Provider = ProviderName,
                    Email = userJson.TryGetProperty("userPrincipalName", out var email) ? email.GetString() ?? "" : "",
                    Name = userJson.TryGetProperty("displayName", out var name) ? name.GetString() : null
                };
            }
            catch
            {
                return null;
            }
        }
    }
}

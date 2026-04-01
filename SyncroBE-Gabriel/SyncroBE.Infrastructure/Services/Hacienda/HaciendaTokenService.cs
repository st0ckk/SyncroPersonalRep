using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyncroBE.Application.Configuration;
using SyncroBE.Application.Interfaces;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    /// <summary>
    /// Manages OAuth 2.0 token lifecycle for the Hacienda API.
    /// Uses password grant to obtain tokens and caches them until near-expiry.
    /// </summary>
    public class HaciendaTokenService : IHaciendaTokenService
    {
        private readonly HaciendaSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HaciendaTokenService> _logger;

        private string? _cachedToken;
        private string? _refreshToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public HaciendaTokenService(
            IOptions<HaciendaSettings> settings,
            HttpClient httpClient,
            ILogger<HaciendaTokenService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetTokenAsync()
        {
            // Return cached token if still valid (with 60s buffer)
            if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry.AddSeconds(-60))
            {
                return _cachedToken;
            }

            // Try refresh if we have a refresh token
            if (_refreshToken != null)
            {
                try
                {
                    return await RefreshTokenAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Token refresh failed, requesting new token via password grant");
                }
            }

            // Password grant (initial authentication)
            return await RequestNewTokenAsync();
        }

        private async Task<string> RequestNewTokenAsync()
        {
            var tokenUrl = _settings.EffectiveTokenUrl;
            var clientId = _settings.EffectiveClientId;

            _logger.LogInformation("Requesting new Hacienda token from {Url}", tokenUrl);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", _settings.ClientSecret),
                new KeyValuePair<string, string>("username", _settings.AtvUsername),
                new KeyValuePair<string, string>("password", _settings.AtvPassword),
            });

            var response = await _httpClient.PostAsync(tokenUrl, content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Hacienda token request failed: {Status} {Body}", response.StatusCode, body);
                throw new HttpRequestException(
                    $"Failed to obtain Hacienda token: {response.StatusCode} - {body}");
            }

            return ParseTokenResponse(body);
        }

        private async Task<string> RefreshTokenAsync()
        {
            var tokenUrl = _settings.EffectiveTokenUrl;
            var clientId = _settings.EffectiveClientId;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("refresh_token", _refreshToken!),
            });

            var response = await _httpClient.PostAsync(tokenUrl, content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _refreshToken = null;  // Invalidate refresh token
                throw new HttpRequestException($"Token refresh failed: {response.StatusCode}");
            }

            return ParseTokenResponse(body);
        }

        private string ParseTokenResponse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            _cachedToken = root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("No access_token in response");

            if (root.TryGetProperty("refresh_token", out var refreshProp))
                _refreshToken = refreshProp.GetString();

            var expiresIn = root.TryGetProperty("expires_in", out var expProp)
                ? expProp.GetInt32()
                : 300; // Default 5 minutes

            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);

            _logger.LogInformation("Hacienda token obtained, expires in {Seconds}s", expiresIn);

            return _cachedToken;
        }
    }
}

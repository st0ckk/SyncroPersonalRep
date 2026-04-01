using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyncroBE.Application.Configuration;
using SyncroBE.Application.Interfaces;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    /// <summary>
    /// Communicates with the Ministerio de Hacienda REST API.
    /// Endpoints:
    ///   POST /recepcion/  → submit document
    ///   GET  /recepcion/{clave} → query status
    /// </summary>
    public class HaciendaApiService : IHaciendaApiService
    {
        private readonly HaciendaSettings _settings;
        private readonly IHaciendaTokenService _tokenService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HaciendaApiService> _logger;

        public HaciendaApiService(
            IOptions<HaciendaSettings> settings,
            IHaciendaTokenService tokenService,
            HttpClient httpClient,
            ILogger<HaciendaApiService> logger)
        {
            _settings = settings.Value;
            _tokenService = tokenService;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(int StatusCode, string ResponseBody)> SendDocumentAsync(
            string clave,
            string fecha,
            string emisorTipoId,
            string emisorId,
            string? receptorTipoId,
            string? receptorId,
            string signedXmlBase64)
        {
            var token = await _tokenService.GetTokenAsync();
            var apiUrl = _settings.EffectiveApiUrl.TrimEnd('/') + "/recepcion/";

            _logger.LogInformation("Sending document to Hacienda: {Clave}", clave);

            // Build the JSON payload per Hacienda API spec
            var payload = new Dictionary<string, object>
            {
                ["clave"] = clave,
                ["fecha"] = fecha,
                ["emisor"] = new
                {
                    tipoIdentificacion = emisorTipoId,
                    numeroIdentificacion = emisorId
                },
                ["comprobanteXml"] = signedXmlBase64
            };

            // Receptor is optional (not present for Tiquete Electrónico without identified buyer)
            if (!string.IsNullOrEmpty(receptorTipoId) && !string.IsNullOrEmpty(receptorId))
            {
                payload["receptor"] = new
                {
                    tipoIdentificacion = receptorTipoId,
                    numeroIdentificacion = receptorId
                };
            }

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync(apiUrl, content);
            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Hacienda response for {Clave}: {Status} {Body}",
                clave, (int)response.StatusCode, body);

            return ((int)response.StatusCode, body);
        }

        public async Task<(int StatusCode, string ResponseBody)> QueryDocumentStatusAsync(string clave)
        {
            var token = await _tokenService.GetTokenAsync();
            var apiUrl = _settings.EffectiveApiUrl.TrimEnd('/') + $"/recepcion/{clave}";

            _logger.LogInformation("Querying Hacienda status for: {Clave}", clave);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(apiUrl);
            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Hacienda status query for {Clave}: {Status}",
                clave, (int)response.StatusCode);

            return ((int)response.StatusCode, body);
        }
    }
}

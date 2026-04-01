using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SyncroBE.Application.DTOs.Hacienda;
using SyncroBE.Application.Interfaces;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    public class HaciendaLookupService : IHaciendaLookupService
    {
        private const string BaseUrl = "https://api.hacienda.go.cr/fe/ae";
        private const string CabysUrl = "https://api.hacienda.go.cr/fe/cabys";
        private readonly HttpClient _httpClient;
        private readonly ILogger<HaciendaLookupService> _logger;

        public HaciendaLookupService(HttpClient httpClient, ILogger<HaciendaLookupService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<HaciendaContributorDto?> LookupContributorAsync(string identificacion)
        {
            try
            {
                var url = $"{BaseUrl}?identificacion={identificacion}";
                _logger.LogInformation("Looking up contributor: {Id}", identificacion);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Hacienda lookup returned {Status} for {Id}",
                        response.StatusCode, identificacion);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<HaciendaContributorDto>();
                _logger.LogInformation("Hacienda lookup OK for {Id}: {Name}", identificacion, result?.Nombre);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up contributor {Id}", identificacion);
                return null;
            }
        }

        public async Task<object?> SearchCabysAsync(string query, int top = 10)
        {
            try
            {
                var encoded = Uri.EscapeDataString(query);
                var url = $"{CabysUrl}?q={encoded}&top={top}";
                _logger.LogInformation("CABYS search: {Query}", query);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CABYS search returned {Status}", response.StatusCode);
                    return null;
                }

                // Return raw JSON from Hacienda
                var json = await response.Content.ReadFromJsonAsync<object>();
                return json;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching CABYS: {Query}", query);
                return null;
            }
        }
    }
}

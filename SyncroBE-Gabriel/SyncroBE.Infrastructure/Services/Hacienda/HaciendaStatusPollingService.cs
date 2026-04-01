using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyncroBE.Application.Configuration;
using SyncroBE.Application.Interfaces;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    /// <summary>
    /// Background service that periodically checks Hacienda for the status of
    /// invoices that are in "sent" or "pending" state.
    /// Runs every 2 minutes by default. Stops polling once an invoice reaches
    /// a terminal state (accepted/rejected) or has been pending for over 48 hours.
    /// </summary>
    public class HaciendaStatusPollingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HaciendaStatusPollingService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _maxAge = TimeSpan.FromHours(48);

        public HaciendaStatusPollingService(
            IServiceProvider serviceProvider,
            ILogger<HaciendaStatusPollingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hacienda status polling service started. Interval: {Interval}",
                _pollingInterval);

            // Wait a bit on startup to let the app initialize
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollPendingInvoicesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during Hacienda status polling cycle");
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }

            _logger.LogInformation("Hacienda status polling service stopped");
        }

        private async Task PollPendingInvoicesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SyncroDbContext>();
            var haciendaApi = scope.ServiceProvider.GetRequiredService<IHaciendaApiService>();

            var cutoffDate = DateTime.UtcNow.Subtract(_maxAge);

            // Find invoices that need status checking
            var pendingInvoices = await db.Invoices
                .Where(i =>
                    (i.HaciendaStatus == "sent" || i.HaciendaStatus == "pending") &&
                    !string.IsNullOrEmpty(i.Clave) &&
                    i.CreatedAt > cutoffDate)
                .OrderBy(i => i.SentAt ?? i.CreatedAt)
                .Take(10) // Process max 10 per cycle to avoid rate limiting
                .ToListAsync(cancellationToken);

            if (!pendingInvoices.Any())
                return;

            _logger.LogInformation("Polling status for {Count} pending invoices", pendingInvoices.Count);

            foreach (var invoice in pendingInvoices)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    var (statusCode, responseBody) = await haciendaApi.QueryDocumentStatusAsync(invoice.Clave!);

                    invoice.ResponseAt = DateTime.UtcNow;

                    if (statusCode == 200)
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("ind-estado", out var estado))
                        {
                            var newStatus = estado.GetString()?.ToLowerInvariant() switch
                            {
                                "aceptado" => "accepted",
                                "rechazado" => "rejected",
                                "procesando" => "sent",
                                _ => invoice.HaciendaStatus
                            };

                            var oldStatus = invoice.HaciendaStatus;
                            invoice.HaciendaStatus = newStatus;

                            if (oldStatus != newStatus)
                            {
                                _logger.LogInformation(
                                    "Invoice {Clave} status changed: {OldStatus} → {NewStatus}",
                                    invoice.Clave, oldStatus, newStatus);
                            }
                        }

                        if (root.TryGetProperty("respuesta-xml", out var respXml))
                        {
                            invoice.XmlResponse = respXml.GetString();
                        }

                        invoice.HaciendaMessage = $"Estado: {invoice.HaciendaStatus} (auto-poll)";
                    }
                    else if (statusCode == 404)
                    {
                        invoice.HaciendaMessage = "Documento no encontrado en Hacienda (auto-poll)";
                    }
                    else
                    {
                        invoice.HaciendaMessage = $"HTTP {statusCode} (auto-poll)";
                    }

                    invoice.UpdatedAt = DateTime.UtcNow;

                    // Small delay between API calls to be respectful to Hacienda's API
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to poll status for invoice {Clave}", invoice.Clave);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}

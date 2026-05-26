using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.Infrastructure.Services
{
    
    //Implementación real de IAuditService.
    //Persiste en la tabla audit_log.
    //Reemplaza a NoOpAuditService.
    
    public class AuditService : IAuditService
    {
        private readonly SyncroDbContext _db;
        private readonly ILogger<AuditService> _logger;

        public AuditService(SyncroDbContext db, ILogger<AuditService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task LogAsync(
            string entityType,
            string entityId,
            string action,
            int userId,
            string? details = null)
        {
            try
            {
                var log = new AuditLog
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    UserId = userId,
                    Details = details,
                    CreatedAt = DateTime.UtcNow
                };

                _db.AuditLogs.Add(log);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // La auditoría nunca debe romper el flujo principal
                _logger.LogError(ex, "Error guardando audit log: {Action} {Entity}:{Id}",
                    action, entityType, entityId);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncroBE.Application.Interfaces;

namespace SyncroBE.Infrastructure.Services
{
    //
    // Implementación temporal que no hace nada.
    // Reemplazar con AuditService real cuando se cree la o las tablas de logs
    //
    public class NoOpAuditService : IAuditService
    {
        public Task LogAsync(string entityType, string entityId, string action, int userId, string? details = null)
        {
            // No-op: listo para implementar con tabla OperationLog
            return Task.CompletedTask;
        }
    }
}
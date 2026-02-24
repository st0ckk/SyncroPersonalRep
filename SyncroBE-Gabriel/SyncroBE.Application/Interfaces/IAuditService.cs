using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.Interfaces
{
    //
    // Interfaz para auditoría de operaciones
    // 
    // Cuando se implemente la(s) tabla(s) para los logs, solo hay que crear la clase real
    // 
    public interface IAuditService
    {
        Task LogAsync(string entityType, string entityId, string action, int userId, string? details = null);
    }
}
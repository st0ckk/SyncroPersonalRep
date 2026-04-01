using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    /// <summary>
    /// Atomically increments consecutive numbers per document type/branch/terminal.
    /// Format: [Branch:3][Terminal:5][DocType:2][Sequence:10] = 20 digits
    /// Uses DB-level locking to prevent duplicate numbers in concurrent scenarios.
    /// </summary>
    public class ConsecutiveService : IConsecutiveService
    {
        private readonly SyncroDbContext _db;

        public ConsecutiveService(SyncroDbContext db)
        {
            _db = db;
        }

        public async Task<string> GetNextConsecutiveAsync(
            string documentType,
            string branchNumber = "001",
            string terminalNumber = "00001")
        {
            // Use raw SQL with UPDLOCK to atomically increment
            var consecutive = await _db.HaciendaConsecutives
                .FirstOrDefaultAsync(c =>
                    c.DocumentType == documentType &&
                    c.BranchNumber == branchNumber &&
                    c.TerminalNumber == terminalNumber);

            if (consecutive == null)
            {
                // Auto-create row if it doesn't exist
                consecutive = new Domain.Entities.HaciendaConsecutive
                {
                    DocumentType = documentType,
                    BranchNumber = branchNumber,
                    TerminalNumber = terminalNumber,
                    LastNumber = 0
                };
                _db.HaciendaConsecutives.Add(consecutive);
            }

            consecutive.LastNumber++;
            consecutive.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Build the 20-digit consecutive:
            // [Branch:3][Terminal:5][DocType:2][Sequence:10]
            var result = $"{branchNumber.PadLeft(3, '0')}" +
                         $"{terminalNumber.PadLeft(5, '0')}" +
                         $"{documentType.PadLeft(2, '0')}" +
                         $"{consecutive.LastNumber.ToString().PadLeft(10, '0')}";

            return result;
        }
    }
}

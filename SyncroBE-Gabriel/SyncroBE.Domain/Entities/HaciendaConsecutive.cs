namespace SyncroBE.Domain.Entities
{
    /// <summary>
    /// Tracks sequential numbering per document type/branch/terminal.
    /// Each combination has its own auto-incrementing counter.
    /// </summary>
    public class HaciendaConsecutive
    {
        public int ConsecutiveId { get; set; }
        public string DocumentType { get; set; } = null!;   // 01=FE, 02=ND, 03=NC, 04=TE, 08=FEC, 09=FEE
        public string BranchNumber { get; set; } = "001";
        public string TerminalNumber { get; set; } = "00001";
        public long LastNumber { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

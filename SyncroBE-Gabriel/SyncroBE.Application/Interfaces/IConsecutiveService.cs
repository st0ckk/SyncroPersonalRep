namespace SyncroBE.Application.Interfaces
{
    /// <summary>
    /// Manages atomic consecutive number generation per document type/branch/terminal.
    /// </summary>
    public interface IConsecutiveService
    {
        /// <summary>
        /// Atomically increments and returns the next 20-digit consecutive number.
        /// Format: [Branch:3][Terminal:5][DocType:2][Sequence:10]
        /// </summary>
        Task<string> GetNextConsecutiveAsync(string documentType, string branchNumber = "001", string terminalNumber = "00001");
    }
}

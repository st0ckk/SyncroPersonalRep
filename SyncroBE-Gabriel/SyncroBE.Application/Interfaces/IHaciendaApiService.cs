namespace SyncroBE.Application.Interfaces
{
    /// <summary>
    /// Communicates with the Ministerio de Hacienda REST API for document submission and status queries.
    /// </summary>
    public interface IHaciendaApiService
    {
        /// <summary>
        /// Submits a signed electronic document to Hacienda.
        /// </summary>
        /// <param name="clave">50-digit document key</param>
        /// <param name="fecha">Emission date in ISO 8601</param>
        /// <param name="emisorTipoId">Issuer ID type</param>
        /// <param name="emisorId">Issuer ID number</param>
        /// <param name="receptorTipoId">Recipient ID type (null for TE)</param>
        /// <param name="receptorId">Recipient ID number (null for TE)</param>
        /// <param name="signedXmlBase64">Base64 encoded signed XML</param>
        /// <returns>HTTP status code and response body</returns>
        Task<(int StatusCode, string ResponseBody)> SendDocumentAsync(
            string clave,
            string fecha,
            string emisorTipoId,
            string emisorId,
            string? receptorTipoId,
            string? receptorId,
            string signedXmlBase64);

        /// <summary>
        /// Queries the status of a previously submitted document.
        /// </summary>
        /// <param name="clave">50-digit document key</param>
        /// <returns>Status response from Hacienda (JSON)</returns>
        Task<(int StatusCode, string ResponseBody)> QueryDocumentStatusAsync(string clave);
    }
}

namespace SyncroBE.Application.Interfaces
{
    /// <summary>
    /// Manages OAuth 2.0 token acquisition and caching for the Hacienda API.
    /// </summary>
    public interface IHaciendaTokenService
    {
        /// <summary>
        /// Returns a valid bearer token, automatically refreshing if expired.
        /// </summary>
        Task<string> GetTokenAsync();
    }
}

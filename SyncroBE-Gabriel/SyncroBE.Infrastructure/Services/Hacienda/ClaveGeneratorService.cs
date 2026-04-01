using SyncroBE.Application.Interfaces;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    /// <summary>
    /// Generates the 50-digit "clave numérica" per Hacienda specification:
    /// [País:3][Fecha:6][Cédula:12][Consecutivo:20][Situación:1][Seguridad:8] = 50 digits
    /// </summary>
    public class ClaveGeneratorService : IClaveGeneratorService
    {
        private static readonly Random _random = new();

        public string Generate(
            string documentType,
            string idType,
            string idNumber,
            string consecutiveNumber,
            string situationCode = "1")
        {
            // 1. Country code: 506 (Costa Rica)
            var country = "506";

            // 2. Date: DDMMYY
            var now = DateTime.Now;  // Local time per Hacienda requirement
            var date = now.ToString("ddMMyy");

            // 3. Identification: padded to 12 digits
            var cedula = idNumber.PadLeft(12, '0');

            // 4. Consecutive: already 20 digits from ConsecutiveService
            var consecutive = consecutiveNumber.PadLeft(20, '0');

            // 5. Situation code: 1 digit (1=Normal)
            var situation = situationCode;

            // 6. Security code: 8 random digits
            var security = _random.Next(10000000, 99999999).ToString();

            var clave = $"{country}{date}{cedula}{consecutive}{situation}{security}";

            if (clave.Length != 50)
                throw new InvalidOperationException(
                    $"Generated clave has {clave.Length} digits instead of 50. " +
                    $"Parts: country={country}, date={date}, cedula={cedula}, " +
                    $"consecutive={consecutive}, situation={situation}, security={security}");

            return clave;
        }
    }
}

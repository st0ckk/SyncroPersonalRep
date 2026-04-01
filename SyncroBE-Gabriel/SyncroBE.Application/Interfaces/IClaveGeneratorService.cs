namespace SyncroBE.Application.Interfaces
{
    /// <summary>
    /// Generates the 50-digit "clave numérica" required by Hacienda.
    /// </summary>
    public interface IClaveGeneratorService
    {
        /// <summary>
        /// Generates a 50-digit clave numérica.
        /// </summary>
        /// <param name="documentType">01=FE, 02=ND, 03=NC, 04=TE</param>
        /// <param name="idType">01=Física, 02=Jurídica, 03=DIMEX, 04=NITE</param>
        /// <param name="idNumber">Cédula/identification number</param>
        /// <param name="consecutiveNumber">20-digit consecutive number</param>
        /// <param name="situationCode">1=Normal, 2=Contingencia, 3=Sin internet</param>
        /// <returns>50-digit clave string</returns>
        string Generate(
            string documentType,
            string idType,
            string idNumber,
            string consecutiveNumber,
            string situationCode = "1");
    }
}

namespace SyncroBE.Application.Interfaces
{
    /// <summary>
    /// Signs XML documents with a .p12 certificate using XAdES-EPES format.
    /// </summary>
    public interface IXmlSignerService
    {
        /// <summary>
        /// Signs an XML string using the configured .p12 certificate.
        /// </summary>
        /// <param name="xml">Unsigned XML document</param>
        /// <returns>Signed XML with ds:Signature element</returns>
        string SignXml(string xml);
    }
}

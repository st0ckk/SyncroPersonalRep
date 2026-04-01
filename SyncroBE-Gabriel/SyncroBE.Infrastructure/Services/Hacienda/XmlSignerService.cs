using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyncroBE.Application.Configuration;
using SyncroBE.Application.Interfaces;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    /// <summary>
    /// Signs XML documents using a .p12 (PKCS#12) certificate with XAdES-EPES.
    /// Uses exclusive C14N, XPath transform, and a Reference to SignedProperties
    /// matching Hacienda's expected signature structure.
    /// </summary>
    public class XmlSignerService : IXmlSignerService
    {
        private readonly HaciendaSettings _settings;
        private readonly ILogger<XmlSignerService> _logger;
        private readonly IWebHostEnvironment _env;

        public XmlSignerService(
            IOptions<HaciendaSettings> settings,
            ILogger<XmlSignerService> logger,
            IWebHostEnvironment env)
        {
            _settings = settings.Value;
            _logger = logger;
            _env = env;
        }

        public string SignXml(string xml)
        {
            _logger.LogInformation("Signing XML document ({Length} chars)", xml.Length);

            // Load the .p12 certificate
            var certPath = _settings.CertificatePath;
            var certPin = _settings.CertificatePin;

            if (!Path.IsPathRooted(certPath))
                certPath = Path.Combine(_env.ContentRootPath, certPath);

            if (!File.Exists(certPath))
                throw new FileNotFoundException($"Certificate file not found: {certPath}");

            var cert = new X509Certificate2(
                certPath,
                certPin,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);

            var rsaKey = cert.GetRSAPrivateKey()
                ?? throw new InvalidOperationException("Certificate does not contain an RSA private key");

            // Load the XML document
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xml);

            // Generate unique IDs for this signature
            var guid = Guid.NewGuid().ToString("N")[..8];
            var signatureId = $"id-{guid}";
            var signedPropsId = $"xades-id-{guid}";
            var signatureValueId = $"value-id-{guid}";
            var docRefId = "r-id-1";

            // Build the XAdES DataObject
            var xadesObject = BuildXadesObject(xmlDoc, cert, signatureId, signedPropsId, docRefId);

            // Create SignedXml with our custom ID resolver
            var signedXml = new XadesSignedXml(xmlDoc)
            {
                SigningKey = rsaKey
            };

            signedXml.Signature.Id = signatureId;
            signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";

            // Reference 1: the entire document content (excluding the Signature itself)
            var docRef = new Reference("")
            {
                Id = docRefId,
                DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256"
            };

            // XPath transform to exclude ds:Signature elements
            var xpathTransform = new XmlDsigXPathTransform();
            var xpathDoc = new XmlDocument();
            var xpathEl = xpathDoc.CreateElement("XPath");
            xpathEl.SetAttribute("xmlns:ds", "http://www.w3.org/2000/09/xmldsig#");
            xpathEl.InnerText = "not(ancestor-or-self::ds:Signature)";
            xpathTransform.LoadInnerXml(xpathEl.SelectNodes(".")!);
            docRef.AddTransform(xpathTransform);
            docRef.AddTransform(new XmlDsigExcC14NTransform());
            signedXml.AddReference(docRef);

            // Reference 2: XAdES SignedProperties
            var propsRef = new Reference("#" + signedPropsId)
            {
                Type = "http://uri.etsi.org/01903#SignedProperties",
                DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256"
            };
            propsRef.AddTransform(new XmlDsigExcC14NTransform());
            signedXml.AddReference(propsRef);

            // KeyInfo with X509 certificate
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;

            // Add the XAdES DataObject (must be added before ComputeSignature)
            signedXml.AddObject(xadesObject);

            // Compute signature
            signedXml.ComputeSignature();

            // Set the SignatureValue Id
            var signatureXml = signedXml.GetXml();
            var svNode = signatureXml.SelectSingleNode(
                "ds:SignatureValue",
                GetDsNsMgr(signatureXml.OwnerDocument));
            if (svNode is XmlElement svEl)
                svEl.SetAttribute("Id", signatureValueId);

            // Append signature to document
            xmlDoc.DocumentElement!.AppendChild(xmlDoc.ImportNode(signatureXml, true));

            _logger.LogInformation("XML signed successfully with XAdES-EPES");
            return xmlDoc.OuterXml;
        }

        private DataObject BuildXadesObject(
            XmlDocument ownerDoc,
            X509Certificate2 cert,
            string signatureId,
            string signedPropsId,
            string docRefId)
        {
            const string xadesNs = "http://uri.etsi.org/01903/v1.3.2#";
            const string dsNs = "http://www.w3.org/2000/09/xmldsig#";

            var tmpDoc = new XmlDocument();

            var qualifyingProps = tmpDoc.CreateElement("xades", "QualifyingProperties", xadesNs);
            qualifyingProps.SetAttribute("Target", "#" + signatureId);

            var signedProps = tmpDoc.CreateElement("xades", "SignedProperties", xadesNs);
            signedProps.SetAttribute("Id", signedPropsId);

            // ── SignedSignatureProperties ──
            var signedSigProps = tmpDoc.CreateElement("xades", "SignedSignatureProperties", xadesNs);

            // SigningTime
            var signingTime = tmpDoc.CreateElement("xades", "SigningTime", xadesNs);
            signingTime.InnerText = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            signedSigProps.AppendChild(signingTime);

            // SigningCertificate
            var signingCertEl = tmpDoc.CreateElement("xades", "SigningCertificate", xadesNs);
            var certEl = tmpDoc.CreateElement("xades", "Cert", xadesNs);

            var certDigest = tmpDoc.CreateElement("xades", "CertDigest", xadesNs);
            var digestMethod = tmpDoc.CreateElement("ds", "DigestMethod", dsNs);
            digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#sha1");
            var digestValue = tmpDoc.CreateElement("ds", "DigestValue", dsNs);
            digestValue.InnerText = Convert.ToBase64String(SHA1.HashData(cert.RawData));
            certDigest.AppendChild(digestMethod);
            certDigest.AppendChild(digestValue);
            certEl.AppendChild(certDigest);

            var issuerSerial = tmpDoc.CreateElement("xades", "IssuerSerial", xadesNs);
            var issuerName = tmpDoc.CreateElement("ds", "X509IssuerName", dsNs);
            issuerName.InnerText = cert.IssuerName.Name;
            var serialNumber = tmpDoc.CreateElement("ds", "X509SerialNumber", dsNs);
            serialNumber.InnerText = new System.Numerics.BigInteger(
                cert.GetSerialNumber().Reverse().ToArray()).ToString();
            issuerSerial.AppendChild(issuerName);
            issuerSerial.AppendChild(serialNumber);
            certEl.AppendChild(issuerSerial);

            signingCertEl.AppendChild(certEl);
            signedSigProps.AppendChild(signingCertEl);

            // SignaturePolicyIdentifier (required by Hacienda XAdES-EPES)
            var policyId = tmpDoc.CreateElement("xades", "SignaturePolicyIdentifier", xadesNs);
            var sigPolicyId = tmpDoc.CreateElement("xades", "SignaturePolicyId", xadesNs);

            var sigPolicyIdInner = tmpDoc.CreateElement("xades", "SigPolicyId", xadesNs);
            var identifier = tmpDoc.CreateElement("xades", "Identifier", xadesNs);
            identifier.InnerText = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/Resolucion_Comprobantes_Electronicos_DGT-R-48-2016.pdf";
            sigPolicyIdInner.AppendChild(identifier);
            sigPolicyId.AppendChild(sigPolicyIdInner);

            var sigPolicyHash = tmpDoc.CreateElement("xades", "SigPolicyHash", xadesNs);
            var policyDigestMethod = tmpDoc.CreateElement("ds", "DigestMethod", dsNs);
            policyDigestMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
            var policyDigestValue = tmpDoc.CreateElement("ds", "DigestValue", dsNs);
            // SHA-256 hash of the policy document identifier
            policyDigestValue.InnerText = Convert.ToBase64String(
                SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
                    "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/Resolucion_Comprobantes_Electronicos_DGT-R-48-2016.pdf")));
            sigPolicyHash.AppendChild(policyDigestMethod);
            sigPolicyHash.AppendChild(policyDigestValue);
            sigPolicyId.AppendChild(sigPolicyHash);

            policyId.AppendChild(sigPolicyId);
            signedSigProps.AppendChild(policyId);

            signedProps.AppendChild(signedSigProps);

            // ── SignedDataObjectProperties ──
            var signedDataObjProps = tmpDoc.CreateElement("xades", "SignedDataObjectProperties", xadesNs);
            var dataObjFormat = tmpDoc.CreateElement("xades", "DataObjectFormat", xadesNs);
            dataObjFormat.SetAttribute("ObjectReference", "#" + docRefId);
            var mimeType = tmpDoc.CreateElement("xades", "MimeType", xadesNs);
            mimeType.InnerText = "application/octet-stream";
            dataObjFormat.AppendChild(mimeType);
            signedDataObjProps.AppendChild(dataObjFormat);
            signedProps.AppendChild(signedDataObjProps);

            qualifyingProps.AppendChild(signedProps);
            tmpDoc.AppendChild(qualifyingProps);

            var dataObject = new DataObject();
            dataObject.Data = tmpDoc.ChildNodes;

            return dataObject;
        }

        private static XmlNamespaceManager GetDsNsMgr(XmlDocument doc)
        {
            var nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            return nsMgr;
        }
    }

    /// <summary>
    /// Custom SignedXml subclass that can resolve IDs inside DataObjects.
    /// .NET's default SignedXml.GetIdElement only searches the main document,
    /// but XAdES puts SignedProperties inside a DataObject which needs to be
    /// found by its Id attribute during signature computation.
    /// </summary>
    internal class XadesSignedXml : SignedXml
    {
        public XadesSignedXml(XmlDocument doc) : base(doc) { }

        public override XmlElement? GetIdElement(XmlDocument? document, string idValue)
        {
            // Try the default implementation first
            var element = base.GetIdElement(document, idValue);
            if (element != null)
                return element;

            // Search within DataObjects for elements with matching Id attribute
            if (document != null)
            {
                // Search the entire document for any element with Id matching
                var nsMgr = new XmlNamespaceManager(document.NameTable);
                nsMgr.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");

                // Look for the xades:SignedProperties element by Id
                var nodes = document.SelectNodes($"//*[@Id='{idValue}']");
                if (nodes != null && nodes.Count > 0)
                    return nodes[0] as XmlElement;
            }

            // Also search the Signature's own Objects
            foreach (DataObject obj in Signature.ObjectList)
            {
                foreach (XmlNode node in obj.Data)
                {
                    var found = FindElementById(node, idValue);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        private static XmlElement? FindElementById(XmlNode node, string idValue)
        {
            if (node is XmlElement el)
            {
                if (el.GetAttribute("Id") == idValue)
                    return el;

                foreach (XmlNode child in el.ChildNodes)
                {
                    var found = FindElementById(child, idValue);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }
    }
}

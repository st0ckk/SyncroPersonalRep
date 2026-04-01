using System.Globalization;
using System.Xml.Linq;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    /// <summary>
    /// Generates Hacienda-compliant XML documents following the FacturaElectronica v4.4 schema.
    /// Supports: exonerations, service vs merchandise classification, unit of measure mapping.
    /// </summary>
    public class XmlGeneratorService : IXmlGeneratorService
    {
        // Hacienda XML namespaces
        private static readonly XNamespace FE_NS =
            "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica";
        private static readonly XNamespace NC_NS =
            "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica";

        // Payment method mappings from your PurchasePaymentMethod to Hacienda codes
        private static readonly Dictionary<string, string> PaymentMethodMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["efectivo"] = "01",
            ["cash"] = "01",
            ["tarjeta"] = "02",
            ["card"] = "02",
            ["cheque"] = "03",
            ["transferencia"] = "04",
            ["transfer"] = "04",
            ["credito"] = "02",
            ["sinpe"] = "04",
        };

        // Hacienda unit of measure codes
        private static readonly Dictionary<string, string> UnitOfMeasureMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["unidad"] = "Unid",
            ["unid"] = "Unid",
            ["servicios_profesionales"] = "Sp",
            ["sp"] = "Sp",
            ["metro"] = "m",
            ["kilogramo"] = "kg",
            ["kg"] = "kg",
            ["litro"] = "L",
            ["hora"] = "h",
            ["otro"] = "Os",
            ["os"] = "Os",
        };

        // Hacienda exoneration document type mapping (text → code)
        private static readonly Dictionary<string, string> ExonerationDocTypeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["01"] = "01",  // Compras autorizadas
            ["02"] = "02",  // Ventas exentas a diplomáticos
            ["03"] = "03",  // Orden de compra
            ["04"] = "04",  // Exenciones DGH
            ["05"] = "05",  // Zonas Francas
            ["06"] = "06",  // Régimen especial
            ["07"] = "07",  // Transitorio
            ["99"] = "99",  // Otros
        };

        public string GenerateInvoiceXml(
            CompanyConfig emisor,
            Client receptor,
            Purchase purchase,
            Invoice invoice)
        {
            var ns = FE_NS;

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                BuildFacturaElectronica(ns, emisor, receptor, purchase, invoice));

            return doc.Declaration + "\n" + doc.Root!.ToString();
        }

        public string GenerateCreditNoteXml(
            CompanyConfig emisor,
            Client receptor,
            Purchase purchase,
            Invoice creditNote,
            Invoice originalInvoice)
        {
            var ns = NC_NS;

            var root = new XElement(ns + "NotaCreditoElectronica",
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"));

            AddCommonElements(root, ns, emisor, receptor, purchase, creditNote);

            // ResumenFactura must come before InformacionReferencia per v4.4 schema
            AddResumenFactura(root, ns, purchase, creditNote, receptor);

            // Add reference to original document (after ResumenFactura per v4.4 schema)
            root.Add(new XElement(ns + "InformacionReferencia",
                new XElement(ns + "TipoDoc", "01"),  // FE
                new XElement(ns + "Numero", originalInvoice.Clave),
                new XElement(ns + "FechaEmision", originalInvoice.EmissionDate?.ToString("yyyy-MM-ddTHH:mm:ss-06:00")),
                new XElement(ns + "Codigo", creditNote.ReferenceCode ?? "01"),
                new XElement(ns + "Razon", creditNote.ReferenceReason ?? "Anulación de documento")));

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
            return doc.Declaration + "\n" + doc.Root!.ToString();
        }

        private XElement BuildFacturaElectronica(
            XNamespace ns,
            CompanyConfig emisor,
            Client receptor,
            Purchase purchase,
            Invoice invoice)
        {
            var root = new XElement(ns + "FacturaElectronica",
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"));

            AddCommonElements(root, ns, emisor, receptor, purchase, invoice);
            AddResumenFactura(root, ns, purchase, invoice, receptor);

            return root;
        }

        private void AddCommonElements(
            XElement root,
            XNamespace ns,
            CompanyConfig emisor,
            Client receptor,
            Purchase purchase,
            Invoice invoice)
        {
            // 1. Clave
            root.Add(new XElement(ns + "Clave", invoice.Clave));

            // 2. ProveedorSistemas (required by v4.4 schema - provider cedula, max 20 chars)
            root.Add(new XElement(ns + "ProveedorSistemas", emisor.IdNumber));

            // 3. CodigoActividadEmisor (pass as-is from DB, e.g. "6202.0")
            root.Add(new XElement(ns + "CodigoActividadEmisor",
                (invoice.ActivityCode ?? emisor.ActivityCode)?.Trim()));

            // 4. CodigoActividadReceptor (optional)
            if (receptor != null && !string.IsNullOrEmpty(receptor.ActivityCode))
            {
                root.Add(new XElement(ns + "CodigoActividadReceptor",
                    receptor.ActivityCode.Trim()));
            }

            // 5. NumeroConsecutivo
            root.Add(new XElement(ns + "NumeroConsecutivo", invoice.ConsecutiveNumber));

            // 6. FechaEmision (ISO 8601 with Costa Rica timezone)
            root.Add(new XElement(ns + "FechaEmision",
                invoice.EmissionDate?.ToString("yyyy-MM-ddTHH:mm:ss-06:00")
                ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss-06:00")));

            // 7. Emisor
            root.Add(BuildEmisor(ns, emisor));

            // 8. Receptor (for FE, receptor is required; for TE it's optional)
            if (receptor != null && !string.IsNullOrEmpty(receptor.ClientId))
            {
                root.Add(BuildReceptor(ns, receptor));
            }

            // 9. CondicionVenta
            root.Add(new XElement(ns + "CondicionVenta",
                invoice.SaleCondition ?? "01"));

            // 10. PlazoCredito (only if credit sale)
            if (invoice.SaleCondition == "02")
            {
                root.Add(new XElement(ns + "PlazoCredito", "30"));
            }

            // 11. DetalleServicio (MedioPago moved to ResumenFactura per v4.4 schema)
            root.Add(BuildDetalleServicio(ns, purchase, receptor));
        }

        private XElement BuildEmisor(XNamespace ns, CompanyConfig emisor)
        {
            var element = new XElement(ns + "Emisor",
                new XElement(ns + "Nombre", emisor.CompanyName),
                new XElement(ns + "Identificacion",
                    new XElement(ns + "Tipo", emisor.IdType),
                    new XElement(ns + "Numero", PadIdNumber(emisor.IdType, emisor.IdNumber))));

            if (!string.IsNullOrEmpty(emisor.CommercialName))
                element.Add(new XElement(ns + "NombreComercial", emisor.CommercialName));

            // Ubicacion (Barrio is optional - omit unless valid 5-digit code exists)
            var emisorCanton = ExtractHacienda2DigitCode(emisor.CantonCode);
            var emisorDistrito = ExtractHacienda2DigitCode(emisor.DistrictCode);
            var ubicacion = new XElement(ns + "Ubicacion",
                new XElement(ns + "Provincia", emisor.ProvinceCode.ToString()),
                new XElement(ns + "Canton", emisorCanton),
                new XElement(ns + "Distrito", emisorDistrito));
            // Only add Barrio if a valid code exists in DB
            if (emisor.NeighborhoodCode.HasValue && emisor.NeighborhoodCode.Value > 0)
            {
                var emisorBarrio = $"{emisor.ProvinceCode}{emisorCanton}{emisor.NeighborhoodCode.Value.ToString().PadLeft(2, '0')}";
                ubicacion.Add(new XElement(ns + "Barrio", emisorBarrio));
            }
            ubicacion.Add(new XElement(ns + "OtrasSenas", emisor.OtherAddress));
            element.Add(ubicacion);

            // Telefono (NumTelefono must be exactly 8 digits for Costa Rica)
            if (!string.IsNullOrEmpty(emisor.PhoneNumber))
            {
                var phone = new string(emisor.PhoneNumber.Where(char.IsDigit).ToArray());
                // CR phone numbers are 8 digits; take last 8 if longer
                if (phone.Length > 8) phone = phone[^8..];
                element.Add(new XElement(ns + "Telefono",
                    new XElement(ns + "CodigoPais", emisor.PhoneCountryCode ?? "506"),
                    new XElement(ns + "NumTelefono", phone.PadLeft(8, '0'))));
            }

            // CorreoElectronico
            element.Add(new XElement(ns + "CorreoElectronico", emisor.Email));

            return element;
        }

        private XElement BuildReceptor(XNamespace ns, Client receptor)
        {
            var element = new XElement(ns + "Receptor",
                new XElement(ns + "Nombre", receptor.ClientName));

            // Only add identification if client has a Hacienda ID type and ID number
            if (!string.IsNullOrEmpty(receptor.HaciendaIdType) &&
                !string.IsNullOrEmpty(receptor.ClientId))
            {
                element.Add(new XElement(ns + "Identificacion",
                    new XElement(ns + "Tipo", receptor.HaciendaIdType),
                    new XElement(ns + "Numero", PadIdNumber(receptor.HaciendaIdType, receptor.ClientId))));
            }

            // Ubicacion (optional for receptor - omit Barrio unless valid)
            if (receptor.ProvinceCode.HasValue &&
                receptor.CantonCode.HasValue &&
                receptor.DistrictCode.HasValue)
            {
                var recCanton = ExtractHacienda2DigitCode(receptor.CantonCode.Value);
                var recDistrito = ExtractHacienda2DigitCode(receptor.DistrictCode.Value);
                var recUbicacion = new XElement(ns + "Ubicacion",
                    new XElement(ns + "Provincia", receptor.ProvinceCode.Value.ToString()),
                    new XElement(ns + "Canton", recCanton),
                    new XElement(ns + "Distrito", recDistrito));
                // Barrio omitted for receptor - not tracked in client entity
                recUbicacion.Add(new XElement(ns + "OtrasSenas", receptor.ExactAddress ?? "No indicado"));
                element.Add(recUbicacion);
            }

            // CorreoElectronico
            if (!string.IsNullOrEmpty(receptor.ClientEmail))
                element.Add(new XElement(ns + "CorreoElectronico", receptor.ClientEmail));

            return element;
        }

        private XElement BuildDetalleServicio(XNamespace ns, Purchase purchase, Client? receptor)
        {
            var detalle = new XElement(ns + "DetalleServicio");
            int lineNumber = 1;

            // Check if client has exoneration
            bool hasExoneration = receptor != null
                && !string.IsNullOrEmpty(receptor.ExonerationDocType)
                && !string.IsNullOrEmpty(receptor.ExonerationDocNumber)
                && receptor.ExonerationPercentage.HasValue
                && receptor.ExonerationPercentage.Value > 0;

            foreach (var item in purchase.SaleDetails)
            {
                var precioUnitario = item.UnitPrice;
                var montoTotal = precioUnitario * item.Quantity;
                var montoDescuento = 0m;

                // Apply purchase-level discount proportionally to each line
                if (purchase.PurchaseDiscountApplied && purchase.PurchaseDiscountPercentage > 0)
                {
                    montoDescuento = Math.Round(montoTotal * purchase.PurchaseDiscountPercentage / 100m, 5);
                }

                var subtotalLinea = montoTotal - montoDescuento;

                // Tax calculation per line
                var montoImpuesto = 0m;
                var montoExoneracion = 0m;
                var impuestoNeto = 0m;
                var baseImponible = subtotalLinea;

                if (purchase.TaxPercentage > 0)
                {
                    montoImpuesto = Math.Round(subtotalLinea * purchase.TaxPercentage / 100m, 5);

                    // If client has exoneration, calculate exoneration amount
                    if (hasExoneration)
                    {
                        var exonerationPct = receptor!.ExonerationPercentage!.Value;
                        montoExoneracion = Math.Round(subtotalLinea * exonerationPct / 100m, 5);
                        impuestoNeto = montoImpuesto - montoExoneracion;
                        if (impuestoNeto < 0) impuestoNeto = 0;
                    }
                    else
                    {
                        impuestoNeto = montoImpuesto;
                    }
                }

                var montoTotalLinea = subtotalLinea + impuestoNeto;

                // Determine unit of measure from product
                var unidadMedida = MapUnitOfMeasure(item.Product?.IsService == true);

                var linea = new XElement(ns + "LineaDetalle",
                    new XElement(ns + "NumeroLinea", lineNumber.ToString()),
                    new XElement(ns + "CodigoCABYS",
                        (item.Product?.CabysCode ?? "0000000000000").PadLeft(13, '0')),
                    new XElement(ns + "Cantidad",
                        item.Quantity.ToString("F3", CultureInfo.InvariantCulture)),
                    new XElement(ns + "UnidadMedida", unidadMedida),
                    new XElement(ns + "Detalle", TruncateString(item.ProductName, 200)),
                    new XElement(ns + "PrecioUnitario",
                        precioUnitario.ToString("F5", CultureInfo.InvariantCulture)),
                    new XElement(ns + "MontoTotal",
                        montoTotal.ToString("F5", CultureInfo.InvariantCulture)));

                // Discount (v4.4: MontoDescuento → CodigoDescuento → NaturalezaDescuento)
                if (montoDescuento > 0)
                {
                    linea.Add(new XElement(ns + "Descuento",
                        new XElement(ns + "MontoDescuento",
                            montoDescuento.ToString("F5", CultureInfo.InvariantCulture)),
                        new XElement(ns + "CodigoDescuento", "07"),  // 07 = Descuento comercial
                        new XElement(ns + "NaturalezaDescuento",
                            TruncateString(purchase.PurchaseDiscountReason ?? "Descuento comercial", 80))));
                }

                linea.Add(new XElement(ns + "SubTotal",
                    subtotalLinea.ToString("F5", CultureInfo.InvariantCulture)));

                // BaseImponible (required by v4.4 schema, even for exempt lines)
                linea.Add(new XElement(ns + "BaseImponible",
                    baseImponible.ToString("F5", CultureInfo.InvariantCulture)));

                // Impuesto (at least one is ALWAYS required after BaseImponible per v4.4)
                if (purchase.TaxPercentage > 0)
                {
                    var taxCode = purchase.Tax?.HaciendaTaxCode ?? "01";
                    var ivaRateCode = purchase.Tax?.HaciendaIvaRateCode ?? "08";
                    var impuesto = new XElement(ns + "Impuesto",
                        new XElement(ns + "Codigo", taxCode),
                        new XElement(ns + "CodigoTarifaIVA", ivaRateCode),
                        new XElement(ns + "Tarifa",
                            purchase.TaxPercentage.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "Monto",
                            montoImpuesto.ToString("F5", CultureInfo.InvariantCulture)));

                    // Exoneration block
                    if (hasExoneration && montoExoneracion > 0)
                    {
                        impuesto.Add(new XElement(ns + "Exoneracion",
                            new XElement(ns + "TipoDocumento",
                                MapExonerationDocType(receptor!.ExonerationDocType!)),
                            new XElement(ns + "NumeroDocumento",
                                receptor.ExonerationDocNumber),
                            new XElement(ns + "NombreInstitucion",
                                receptor.ExonerationInstitutionName ?? "Ministerio de Hacienda"),
                            new XElement(ns + "FechaEmision",
                                receptor.ExonerationDate?.ToString("yyyy-MM-ddTHH:mm:ss-06:00")
                                ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss-06:00")),
                            new XElement(ns + "PorcentajeExoneracion",
                                receptor.ExonerationPercentage!.Value.ToString()),
                            new XElement(ns + "MontoExoneracion",
                                montoExoneracion.ToString("F5", CultureInfo.InvariantCulture))));
                    }

                    linea.Add(impuesto);

                    // ImpuestoAsumidoEmisorFabrica (required by v4.4 - 0 if not applicable)
                    linea.Add(new XElement(ns + "ImpuestoAsumidoEmisorFabrica",
                        "0.00000"));

                    // ImpuestoNeto (required by v4.4)
                    linea.Add(new XElement(ns + "ImpuestoNeto",
                        impuestoNeto.ToString("F5", CultureInfo.InvariantCulture)));
                }
                else
                {
                    // Exempt line: Impuesto with tarifa 0% (CodigoTarifaIVA=01) is required by v4.4
                    linea.Add(new XElement(ns + "Impuesto",
                        new XElement(ns + "Codigo", "01"),
                        new XElement(ns + "CodigoTarifaIVA", "01"),
                        new XElement(ns + "Tarifa", "0.00"),
                        new XElement(ns + "Monto", "0.00000")));

                    // Both required by v4.4 even for exempt lines
                    linea.Add(new XElement(ns + "ImpuestoAsumidoEmisorFabrica", "0.00000"));
                    linea.Add(new XElement(ns + "ImpuestoNeto", "0.00000"));
                }

                linea.Add(new XElement(ns + "MontoTotalLinea",
                    montoTotalLinea.ToString("F5", CultureInfo.InvariantCulture)));

                detalle.Add(linea);
                lineNumber++;
            }

            return detalle;
        }

        private void AddResumenFactura(
            XElement root,
            XNamespace ns,
            Purchase purchase,
            Invoice invoice,
            Client? receptor)
        {
            // Categorized totals per Hacienda requirements
            var totalServGravados = 0m;
            var totalServExentos = 0m;
            var totalServExonerado = 0m;
            var totalMercGravadas = 0m;
            var totalMercExentas = 0m;
            var totalMercExonerada = 0m;
            var totalServNoSujeto = 0m;
            var totalMercNoSujeta = 0m;
            var totalDescuentos = 0m;
            var totalImpuestos = 0m;
            var totalExoneracion = 0m;
            var totalImpuestoNeto = 0m;

            bool hasExoneration = receptor != null
                && !string.IsNullOrEmpty(receptor.ExonerationDocType)
                && receptor.ExonerationPercentage.HasValue
                && receptor.ExonerationPercentage.Value > 0;

            foreach (var item in purchase.SaleDetails)
            {
                // montoTotal = PrecioUnitario * Cantidad (pre-discount, this goes into TotalMerc/TotalServ)
                var montoTotal = item.UnitPrice * item.Quantity;
                var descuento = 0m;

                if (purchase.PurchaseDiscountApplied && purchase.PurchaseDiscountPercentage > 0)
                {
                    descuento = Math.Round(montoTotal * purchase.PurchaseDiscountPercentage / 100m, 5);
                    totalDescuentos += descuento;
                }

                // subtotalLinea = after discount (used for tax calculation)
                var subtotalLinea = montoTotal - descuento;
                bool isService = item.Product?.IsService == true;

                if (purchase.TaxPercentage > 0)
                {
                    var impuesto = Math.Round(subtotalLinea * purchase.TaxPercentage / 100m, 5);

                    if (hasExoneration)
                    {
                        var exonerationAmt = Math.Round(subtotalLinea * receptor!.ExonerationPercentage!.Value / 100m, 5);
                        var neto = impuesto - exonerationAmt;
                        if (neto < 0) neto = 0;

                        totalExoneracion += exonerationAmt;
                        totalImpuestoNeto += neto;
                        totalImpuestos += impuesto;

                        // Use montoTotal (pre-discount) for category totals
                        if (isService)
                            totalServExonerado += montoTotal;
                        else
                            totalMercExonerada += montoTotal;
                    }
                    else
                    {
                        totalImpuestos += impuesto;
                        totalImpuestoNeto += impuesto;

                        // Use montoTotal (pre-discount) for category totals
                        if (isService)
                            totalServGravados += montoTotal;
                        else
                            totalMercGravadas += montoTotal;
                    }
                }
                else
                {
                    // Use montoTotal (pre-discount) for category totals
                    if (isService)
                        totalServNoSujeto += montoTotal;
                    else
                        totalMercNoSujeta += montoTotal;
                }
            }

            var totalGravado = totalServGravados + totalMercGravadas;
            var totalExento = totalServExentos + totalMercExentas;
            var totalExonerado = totalServExonerado + totalMercExonerada;
            var totalNoSujeto = totalServNoSujeto + totalMercNoSujeta;
            var totalVenta = totalGravado + totalExento + totalExonerado + totalNoSujeto;
            var totalVentaNeta = totalVenta - totalDescuentos;
            var totalComprobante = totalVentaNeta + totalImpuestoNeto;

            var resumen = new XElement(ns + "ResumenFactura",
                new XElement(ns + "CodigoTipoMoneda",
                    new XElement(ns + "CodigoMoneda", invoice.CurrencyCode ?? "CRC"),
                    new XElement(ns + "TipoCambio",
                        (invoice.ExchangeRate ?? 1m).ToString("F5", CultureInfo.InvariantCulture))));

            // Services
            if (totalServGravados > 0)
                resumen.Add(new XElement(ns + "TotalServGravados",
                    totalServGravados.ToString("F5", CultureInfo.InvariantCulture)));
            if (totalServExentos > 0)
                resumen.Add(new XElement(ns + "TotalServExentos",
                    totalServExentos.ToString("F5", CultureInfo.InvariantCulture)));
            if (totalServExonerado > 0)
                resumen.Add(new XElement(ns + "TotalServExonerado",
                    totalServExonerado.ToString("F5", CultureInfo.InvariantCulture)));
            if (totalServNoSujeto > 0)
                resumen.Add(new XElement(ns + "TotalServNoSujeto",
                    totalServNoSujeto.ToString("F5", CultureInfo.InvariantCulture)));

            // Merchandise
            if (totalMercGravadas > 0)
                resumen.Add(new XElement(ns + "TotalMercanciasGravadas",
                    totalMercGravadas.ToString("F5", CultureInfo.InvariantCulture)));
            if (totalMercExentas > 0)
                resumen.Add(new XElement(ns + "TotalMercanciasExentas",
                    totalMercExentas.ToString("F5", CultureInfo.InvariantCulture)));
            if (totalMercExonerada > 0)
                resumen.Add(new XElement(ns + "TotalMercExonerada",
                    totalMercExonerada.ToString("F5", CultureInfo.InvariantCulture)));
            if (totalMercNoSujeta > 0)
                resumen.Add(new XElement(ns + "TotalMercNoSujeta",
                    totalMercNoSujeta.ToString("F5", CultureInfo.InvariantCulture)));

            // Aggregated totals
            if (totalGravado > 0)
                resumen.Add(new XElement(ns + "TotalGravado",
                    totalGravado.ToString("F5", CultureInfo.InvariantCulture)));
            if (totalExento > 0)
                resumen.Add(new XElement(ns + "TotalExento",
                    totalExento.ToString("F5", CultureInfo.InvariantCulture)));
            if (totalExonerado > 0)
                resumen.Add(new XElement(ns + "TotalExonerado",
                    totalExonerado.ToString("F5", CultureInfo.InvariantCulture)));
            if (totalNoSujeto > 0)
                resumen.Add(new XElement(ns + "TotalNoSujeto",
                    totalNoSujeto.ToString("F5", CultureInfo.InvariantCulture)));

            resumen.Add(new XElement(ns + "TotalVenta",
                totalVenta.ToString("F5", CultureInfo.InvariantCulture)));

            if (totalDescuentos > 0)
                resumen.Add(new XElement(ns + "TotalDescuentos",
                    totalDescuentos.ToString("F5", CultureInfo.InvariantCulture)));

            resumen.Add(new XElement(ns + "TotalVentaNeta",
                totalVentaNeta.ToString("F5", CultureInfo.InvariantCulture)));

            // TotalDesgloseImpuesto (tax breakdown - required when taxes exist)
            if (totalImpuestos > 0)
            {
                var taxCode = purchase.Tax?.HaciendaTaxCode ?? "01";
                var ivaRateCode = purchase.Tax?.HaciendaIvaRateCode ?? "08";
                resumen.Add(new XElement(ns + "TotalDesgloseImpuesto",
                    new XElement(ns + "Codigo", taxCode),
                    new XElement(ns + "CodigoTarifaIVA", ivaRateCode),
                    new XElement(ns + "TotalMontoImpuesto",
                        totalImpuestos.ToString("F5", CultureInfo.InvariantCulture))));
            }

            if (totalImpuestos > 0)
                resumen.Add(new XElement(ns + "TotalImpuesto",
                    totalImpuestos.ToString("F5", CultureInfo.InvariantCulture)));

            if (totalExoneracion > 0)
                resumen.Add(new XElement(ns + "TotalIVADevuelto",
                    "0.00000"));

            if (totalExoneracion > 0)
                resumen.Add(new XElement(ns + "TotalOtrosCargos",
                    "0.00000"));

            // MedioPago (1-4 occurrences, complex type per v4.4 schema)
            var paymentCode = MapPaymentMethod(purchase.PurchasePaymentMethod);
            resumen.Add(new XElement(ns + "MedioPago",
                new XElement(ns + "TipoMedioPago", paymentCode),
                new XElement(ns + "TotalMedioPago",
                    totalComprobante.ToString("F5", CultureInfo.InvariantCulture))));

            resumen.Add(new XElement(ns + "TotalComprobante",
                totalComprobante.ToString("F5", CultureInfo.InvariantCulture)));

            root.Add(resumen);
        }

        private static string MapPaymentMethod(string? method)
        {
            if (string.IsNullOrEmpty(method)) return "01"; // Default: efectivo
            return PaymentMethodMap.TryGetValue(method.Trim(), out var code) ? code : "99";
        }

        private static string MapUnitOfMeasure(bool isService)
        {
            return isService ? "Sp" : "Unid";
        }

        private static string MapExonerationDocType(string docType)
        {
            if (ExonerationDocTypeMap.TryGetValue(docType, out var code))
                return code;
            return docType; // Return as-is if already a valid code
        }

        private static string TruncateString(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return "Producto";
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        /// <summary>
        /// Pads an identification number based on the Hacienda ID type:
        /// 01 (Física) = 9 digits, 02 (Jurídica) = 10 digits,
        /// 03 (DIMEX) = 12 digits, 04 (NITE) = 10 digits
        /// </summary>
        private static string PadIdNumber(string idType, string idNumber)
        {
            // Strip any non-digit characters
            var digits = new string(idNumber.Where(char.IsDigit).ToArray());
            var padLength = idType switch
            {
                "01" => 9,   // Cédula Física
                "02" => 10,  // Cédula Jurídica
                "03" => 12,  // DIMEX
                "04" => 10,  // NITE
                _ => 12
            };
            return digits.PadLeft(padLength, '0');
        }

        /// <summary>
        /// Formats an activity code: removes decimals and pads to 6 digits.
        /// DB may store as "6202.0" but Hacienda expects "620200".
        /// </summary>
        private static string FormatActivityCode(string code)
        {
            // Remove any decimal points and trailing zeros from decimal format
            var dotIndex = code.IndexOf('.');
            if (dotIndex >= 0)
            {
                // e.g. "6202.0" → "6202" then pad to 6 → "620200"
                code = code[..dotIndex];
            }
            return code.PadRight(6, '0');
        }

        /// <summary>
        /// Extracts the Hacienda 2-digit code from a composite DB code.
        /// DB stores Canton as e.g. 301 (province 3 + canton 01) and District as 30101.
        /// Hacienda expects just the 2-digit canton/district portion.
        /// </summary>
        private static string ExtractHacienda2DigitCode(int compositeCode)
        {
            var str = compositeCode.ToString();
            // Take last 2 digits from the composite code
            return str.Length >= 2 ? str[^2..] : str.PadLeft(2, '0');
        }
    }
}

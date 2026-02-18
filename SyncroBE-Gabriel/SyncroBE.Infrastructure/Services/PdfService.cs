using HtmlAgilityPack;
using SyncroBE.Application.DTOs.Quote;
using SyncroBE.Application.DTOs.QuoteDetails;
using SyncroBE.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        public async Task<string> GenerateQuotePdfCopy(QuoteDto quote)
        {
            string templateFileContent;

            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            //Ubica y lee el nombre de la plantilla HTML
            string htmlFileName = "Templates\\Quote.html";
            string filePath = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), htmlFileName);
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                templateFileContent = reader.ReadToEnd();
            }

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(templateFileContent);
            HtmlNode htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//div");

            if(htmlNode == null)
            {
                return string.Empty;
            }

            string headerHtmlContent = GetQuoteHeaderDetails(quote);
            string bodyHtmlContent = GetQuoteBodyDetails(quote);
            string bottomHtmlContent = GetQuoteBottomDetails(quote);
            htmlNode.InnerHtml = headerHtmlContent + bodyHtmlContent + bottomHtmlContent;

            string updateHtml = htmlDocument.DocumentNode.OuterHtml;

            return updateHtml;
        }

        /*-------------------------------------------Contenido de cotizacion-------------------------------------------*/
        private string GetQuoteHeaderDetails(QuoteDto quote)
        {
            string htmlHeaderDetails = $@"<div class=""px-14 py-6"">
                <table class=""w-full border-collapse border-spacing-0"">
                    <tbody>
                        <tr>
                            <td class=""w-full align-top"">
                                <div>
                                    <img src=""data:image/jpeg;base64,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"" 
                                         class=""h-12""
                                         style=""height: 150px; width: 150px""/>
                                </div>
                            </td>

                            <td class=""align-top"">
                                <div class=""text-sm"">
                                    <table class=""border-collapse border-spacing-0"">
                                        <tbody>
                                            <tr>
                                                <td class=""border-r pr-4"">
                                                    <div>
                                                        <p class=""whitespace-nowrap text-slate-400 text-right"">Fecha de emisión</p>
                                                        <p class=""whitespace-nowrap font-bold text-main text-right"">{quote.QuoteDate.Date.ToShortDateString()}</p>
                                                    </div>
                                                </td>
                                                <td class=""pl-4"">
                                                    <div>
                                                        <p class=""whitespace-nowrap text-slate-400 text-right"">Cotizacion #</p>
                                                        <p class=""whitespace-nowrap font-bold text-main text-right"">{quote.QuoteNumber}</p>
                                                    </div>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class=""px-14 py-6 text-sm"">
                <table class=""w-full border-collapse border-spacing-0"">
                    <tbody>
                        <tr>
                            <td class=""w-1/2 align-top"">
                                <div class=""text-sm text-neutral-600"">
                                    <p class=""font-bold"">Detalles de Cotización</p>
                                    <p>Cliente: {quote.ClientName}</p>
                                    <p>Cotizador: {quote.UserName}</p>
                                    <p>Valido hasta: {quote.QuoteValidTil.Date.ToShortDateString()}</p>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>";
            return htmlHeaderDetails;
        }
        private string GetQuoteBodyDetails(QuoteDto quote)
        {
            int counter = 0;
            StringBuilder htmlBodyDetails = new StringBuilder();
            htmlBodyDetails.Append(@"
                    <div class=""px-14 py-10 text-sm text-neutral-700"">
                    <table class=""w-full border-collapse border-spacing-0"">
                    <thead>
                        <tr>
                            <td class=""border-b-2 border-main pb-3 pl-3 font-bold text-main"">Item</td>
                            <td class=""border-b-2 border-main pb-3 pl-2 font-bold text-main"">Descripción</td>
                            <td class=""border-b-2 border-main pb-3 pl-2 text-right font-bold text-main"">Cantidad</td>
                            <td class=""border-b-2 border-main pb-3 pl-2 text-center font-bold text-main"">Precio Unitario</td>
                            <td class=""border-b-2 border-main pb-3 pl-2 text-center font-bold text-main"">Subtotal</td>
                        </tr>
                    </thead>
                    <tbody>");
            foreach(QuoteDetailDto qd in quote.QuoteDetails)
            {
                htmlBodyDetails.Append($@"
                        <tr>
                            <td class=""border-b py-3 pl-3"">{counter++}</td>
                            <td class=""border-b py-3 pl-2"">{qd.ProductName}</td>
                            <td class=""border-b py-3 pl-2 text-right"">{qd.Quantity}</td>
                            <td class=""border-b py-3 pl-2 text-right"">₡{qd.UnitPrice}</td>
                            <td class=""border-b py-3 pl-2 pr-3 text-right"">₡{qd.LineTotal}</td>
                        </tr>
                ");
            }
            htmlBodyDetails.Append($@"                        
                          <tr>
                            <td colspan=""7"">
                                <table class=""w-full border-collapse border-spacing-0"">
                                    <tbody>
                                        <tr>
                                            <td class=""w-full""></td>
                                            <td>
                                                <table class=""w-full border-collapse border-spacing-0"">
                                                    <tbody>
<!--
                                                        <tr>
                                                            <td class=""border-b p-3"">
                                                                <div class=""whitespace-nowrap text-slate-400"">Total:</div>
                                                            </td>
                                                            <td class=""border-b p-3 text-right"">
                                                                <div class=""whitespace-nowrap font-bold text-main"">$320.00</div>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class=""p-3"">
                                                                <div class=""whitespace-nowrap text-slate-400"">VAT total:</div>
                                                            </td>
                                                            <td class=""p-3 text-right"">
                                                                <div class=""whitespace-nowrap font-bold text-main"">$64.00</div>
                                                            </td>
                                                        </tr>
-->
                                                        </br >
                                                        <tr>
                                                            <td class=""bg-main p-3"">
                                                                <div class=""whitespace-nowrap font-bold text-white"">Total:</div>
                                                            </td>
                                                            <td class=""bg-main p-3 text-right"">
                                                                <div class=""whitespace-nowrap font-bold text-white"">₡{quote.QuoteTotal}</div>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>");
            return htmlBodyDetails.ToString();
        }
        private string GetQuoteBottomDetails(QuoteDto quote)
        {
            string htmlBottomDetails = $@"
            <div class=""px-14 text-sm text-neutral-700"">
                <p class=""text-main font-bold"">Condiciones y Terminos</p>
                <p>
                    {quote.QuoteConditions}
                </p>
            </div>

            <div class=""px-14 py-10 text-sm text-neutral-700"">
                <p class=""text-main font-bold"">Observaciones</p>
                <p class=""italic"">
                    {quote.QuoteRemarks}
                </p>
                </div>

                <footer class=""fixed bottom-0 left-0 bg-slate-100 w-full text-neutral-600 text-center text-xs py-3"">
                    Syncro
                    <span class=""text-slate-300 px-2"">|</span>
                    All rights reserved
                </footer>";
            return htmlBottomDetails;
        }
    }
}

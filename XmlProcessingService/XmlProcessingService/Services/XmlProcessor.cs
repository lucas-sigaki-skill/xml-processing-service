using System.Xml.Linq;
using XmlProcessingService.Models;

namespace XmlProcessingService.Services
{
    public class XmlProcessor
    {
        private readonly ILogger<XmlProcessor> _logger;
        private readonly DbHelper _dbHelper;

        public XmlProcessor(ILogger<XmlProcessor> logger, DbHelper dbHelper)
        {
            _logger = logger;
            _dbHelper = dbHelper;
        }

        public async Task<bool> ProcessXmlFileAsync(string filePath)
        {
            try
            {
                _logger.LogInformation($"Processing XML file: {filePath}");

                // Load XML document
                var xmlDoc = XDocument.Load(filePath);
                
                // Detect XML type and extract data
                var xmlType = DetectXmlType(xmlDoc);
                _logger.LogInformation($"Detected XML type: {xmlType}");

                switch (xmlType)
                {
                    case XmlType.NFCe:
                        return await ProcessNFCeAsync(xmlDoc, filePath);
                    case XmlType.SATCFe:
                        return await ProcessSATCFeAsync(xmlDoc, filePath);
                    default:
                        _logger.LogWarning($"Unknown XML type for file: {filePath}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing XML file: {filePath}");
                return false;
            }
        }

        private XmlType DetectXmlType(XDocument xmlDoc)
        {
            // Check for NFCe structure
            if (xmlDoc.Root?.Name.LocalName == "nfeProc" && 
                xmlDoc.Root.Elements().Any(e => e.Name.LocalName == "NFe"))
            {
                return XmlType.NFCe;
            }

            // Check for SAT CFe structure
            if (xmlDoc.Root?.Name.LocalName == "CFe" && 
                xmlDoc.Root.Elements().Any(e => e.Name.LocalName == "infCFe"))
            {
                return XmlType.SATCFe;
            }

            return XmlType.Unknown;
        }

        private async Task<bool> ProcessNFCeAsync(XDocument xmlDoc, string filePath)
        {
            try
            {
                var nfe = xmlDoc.Root?.Element(XName.Get("NFe", "http://www.portalfiscal.inf.br/nfe"));
                var infNFe = nfe?.Element(XName.Get("infNFe", "http://www.portalfiscal.inf.br/nfe"));
                var emit = infNFe?.Element(XName.Get("emit", "http://www.portalfiscal.inf.br/nfe"));
                var total = infNFe?.Element(XName.Get("total", "http://www.portalfiscal.inf.br/nfe"));
                var icmsTot = total?.Element(XName.Get("ICMSTot", "http://www.portalfiscal.inf.br/nfe"));

                var nfceData = new NFCeDocument
                {
                    ChaveNFe = infNFe?.Attribute("Id")?.Value ?? "",
                    CNPJ = emit?.Element(XName.Get("CNPJ", "http://www.portalfiscal.inf.br/nfe"))?.Value ?? "",
                    RazaoSocial = emit?.Element(XName.Get("xNome", "http://www.portalfiscal.inf.br/nfe"))?.Value ?? "",
                    DataEmissao = DateTime.TryParse(infNFe?.Element(XName.Get("ide", "http://www.portalfiscal.inf.br/nfe"))?.Element(XName.Get("dhEmi", "http://www.portalfiscal.inf.br/nfe"))?.Value, out var dataEmissao) ? dataEmissao : DateTime.MinValue,
                    ValorTotal = decimal.TryParse(icmsTot?.Element(XName.Get("vNF", "http://www.portalfiscal.inf.br/nfe"))?.Value, out var valorTotal) ? valorTotal : 0,
                    ArquivoOrigem = Path.GetFileName(filePath),
                    DataProcessamento = DateTime.Now
                };

                return await _dbHelper.InsertNFCeDocumentAsync(nfceData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing NFCe: {filePath}");
                return false;
            }
        }

        private async Task<bool> ProcessSATCFeAsync(XDocument xmlDoc, string filePath)
        {
            try
            {
                var infCFe = xmlDoc.Root?.Element("infCFe");
                var emit = infCFe?.Element("emit");
                var total = infCFe?.Element("total");

                var satData = new SATCFeDocument
                {
                    ChaveCFe = infCFe?.Attribute("Id")?.Value ?? "",
                    CNPJ = emit?.Element("CNPJ")?.Value ?? "",
                    RazaoSocial = emit?.Element("xNome")?.Value ?? "",
                    DataEmissao = DateTime.TryParseExact($"{infCFe?.Element("ide")?.Element("dEmi")?.Value} {infCFe?.Element("ide")?.Element("hEmi")?.Value}", 
                        "yyyyMMdd HHmmss", null, System.Globalization.DateTimeStyles.None, out var dataEmissao) ? dataEmissao : DateTime.MinValue,
                    ValorTotal = decimal.TryParse(total?.Element("vCFe")?.Value, out var valorTotal) ? valorTotal : 0,
                    ArquivoOrigem = Path.GetFileName(filePath),
                    DataProcessamento = DateTime.Now
                };

                return await _dbHelper.InsertSATCFeDocumentAsync(satData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing SAT CFe: {filePath}");
                return false;
            }
        }
    }

    public enum XmlType
    {
        Unknown,
        NFCe,
        SATCFe
    }

    public class NFCeDocument
    {
        public string ChaveNFe { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string RazaoSocial { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }
        public decimal ValorTotal { get; set; }
        public string ArquivoOrigem { get; set; } = string.Empty;
        public DateTime DataProcessamento { get; set; }
    }

    public class SATCFeDocument
    {
        public string ChaveCFe { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string RazaoSocial { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }
        public decimal ValorTotal { get; set; }
        public string ArquivoOrigem { get; set; } = string.Empty;
        public DateTime DataProcessamento { get; set; }
    }
}

using System;

namespace Operativ.WebServices.Modelos;
public class FacturaXml
{
    public string NumeroFactura { get; set; }

    public string RazonSocial { get; set; }

    public string Cuit { get; set; }

    public string Email { get; set; }

    public string NombrePlan { get; set; }

    public decimal Total { get; set; }

    public string Moneda { get; set; }

    public DateTime FechaEmision { get; set; }

    public string CodigoComprobantePago { get; set; }

    public string NombreArchivoXml { get; set; }

    public string FacturaHtml { get; set; }
}

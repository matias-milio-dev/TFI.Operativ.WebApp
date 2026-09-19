using System;

namespace Operativ.WebServices.Modelos;
public class ComprobantePagoXml
{
    public string CodigoComprobante { get; set; }

    public int IdSuscripcion { get; set; }

    public string RazonSocial { get; set; }

    public string Cuit { get; set; }

    public string NombrePlan { get; set; }

    public decimal Importe { get; set; }

    public string Moneda { get; set; }

    public string MedioPago { get; set; }

    public DateTime FechaPago { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public string NombreArchivoXml { get; set; }

    public string ComprobanteHtml { get; set; }
}

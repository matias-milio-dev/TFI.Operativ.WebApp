using System;

namespace Operativ.WebServices.Modelos;
public class ResumenSuscripcionXml
{
    public int IdSuscripcion { get; set; }

    public string RazonSocial { get; set; }

    public string Cuit { get; set; }

    public string EmailContacto { get; set; }

    public string NombrePlan { get; set; }

    public string DescripcionPlan { get; set; }

    public decimal PrecioAnual { get; set; }

    public string Estado { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaFinTrial { get; set; }

    public int DiasTrialRestantes { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public string NombreArchivoXml { get; set; }

    public string ResumenHtml { get; set; }
}

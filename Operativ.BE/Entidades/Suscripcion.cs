using System;
using Operativ.BE.Enums;

namespace Operativ.BE.Entidades;
public class Suscripcion
{
    public int IdSuscripcion { get; set; }

    public int IdCliente { get; set; }

    public string RazonSocialCliente { get; set; }

    public string CuitCliente { get; set; }

    public string EmailCliente { get; set; }

    public int IdPlan { get; set; }

    public string NombrePlan { get; set; }

    public string DescripcionPlan { get; set; }

    public decimal PrecioAnual { get; set; }

    public EstadoSuscripcion Estado { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaFinTrial { get; set; }

    public DateTime? FechaPago { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public DateTime? FechaCancelacion { get; set; }

    public string MedioPago { get; set; }

    public string CodigoComprobante { get; set; }

    public bool EstaPendienteDePago
    {
        get { return Estado == EstadoSuscripcion.PendientePago; }
    }

    public bool EstaActiva
    {
        get { return Estado == EstadoSuscripcion.Activa; }
    }

    public bool EsVigente
    {
        get { return EstaActiva || EstaPendienteDePago; }
    }

    public int DiasTrialRestantes
    {
        get
        {
            int dias = (FechaFinTrial.Date - DateTime.Now.Date).Days;

            if (dias < 0)
            {
                return 0;
            }

            return dias;
        }
    }
}

using System;

namespace Operativ.BE.Entidades;
public class Factura
{
    public int IdFactura { get; set; }

    public string NumeroFactura { get; set; }

    public int IdSuscripcion { get; set; }

    public decimal Total { get; set; }

    public string Moneda { get; set; }

    public DateTime FechaEmision { get; set; }

    public int IdCliente { get; set; }

    public string RazonSocialCliente { get; set; }

    public string CuitCliente { get; set; }

    public string EmailCliente { get; set; }

    public string NombrePlan { get; set; }

    public string CodigoComprobantePago { get; set; }
}

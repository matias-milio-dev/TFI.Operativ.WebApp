using System;

namespace Operativ.BE.Entidades;
public class Factura
{
    public int IdFactura { get; set; }

    public string NumeroFactura { get; set; }

    public int IdSuscripcion { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public decimal Iva
    {
        get { return Total - Subtotal; }
    }

    public decimal PorcentajeIva
    {
        get { return Subtotal == 0 ? 0 : Math.Round(Iva * 100m / Subtotal, 2); }
    }

    public string Moneda { get; set; }

    public DateTime FechaEmision { get; set; }

    public int IdCliente { get; set; }

    public string RazonSocialCliente { get; set; }

    public string CuitCliente { get; set; }

    public string EmailCliente { get; set; }

    public string NombrePlan { get; set; }

    public string CodigoComprobantePago { get; set; }
}

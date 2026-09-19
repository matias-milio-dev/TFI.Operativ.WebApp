using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Convertidores;
public static class FacturaConvertidor
{
    public static Factura ToFactura(this DataRow fila)
    {
        Factura factura = new Factura
        {
            IdFactura = (int)fila["IdFactura"],
            NumeroFactura = fila["NumeroFactura"].ToString(),
            IdSuscripcion = (int)fila["IdSuscripcion"],
            Total = (decimal)fila["Total"],
            Moneda = fila["Moneda"].ToString(),
            FechaEmision = (DateTime)fila["FechaEmision"]
        };

        if (fila.Table.Columns.Contains("RazonSocialCliente"))
        {
            factura.IdCliente = (int)fila["IdCliente"];
            factura.RazonSocialCliente = fila["RazonSocialCliente"].ToString();
            factura.CuitCliente = fila["CuitCliente"].ToString();
            factura.EmailCliente = fila["EmailCliente"].ToString();
            factura.NombrePlan = fila["NombrePlan"].ToString();
            factura.CodigoComprobantePago = fila["CodigoComprobantePago"] == DBNull.Value ? null : fila["CodigoComprobantePago"].ToString();
        }

        return factura;
    }

    public static List<Factura> ToListaFacturas(this DataTable tabla)
    {
        List<Factura> facturas = new List<Factura>();

        foreach (DataRow fila in tabla.Rows)
        {
            facturas.Add(fila.ToFactura());
        }

        return facturas;
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.DAL.Convertidores;
public static class SuscripcionConvertidor
{
    public static Suscripcion ToSuscripcion(this DataRow fila)
    {
        Suscripcion suscripcion = new Suscripcion
        {
            IdSuscripcion = (int)fila["IdSuscripcion"],
            IdCliente = (int)fila["IdCliente"],
            IdPlan = (int)fila["IdPlan"],
            PrecioAnual = (decimal)fila["PrecioAnual"],
            Estado = (EstadoSuscripcion)Enum.Parse(typeof(EstadoSuscripcion), fila["Estado"].ToString()),
            FechaAlta = (DateTime)fila["FechaAlta"],
            FechaFinTrial = (DateTime)fila["FechaFinTrial"],
            FechaPago = fila["FechaPago"] == DBNull.Value ? (DateTime?)null : (DateTime)fila["FechaPago"],
            FechaVencimiento = fila["FechaVencimiento"] == DBNull.Value ? (DateTime?)null : (DateTime)fila["FechaVencimiento"],
            FechaCancelacion = fila["FechaCancelacion"] == DBNull.Value ? (DateTime?)null : (DateTime)fila["FechaCancelacion"],
            MedioPago = fila["MedioPago"] == DBNull.Value ? null : fila["MedioPago"].ToString(),
            CodigoComprobante = fila["CodigoComprobante"] == DBNull.Value ? null : fila["CodigoComprobante"].ToString()
        };

        if (fila.Table.Columns.Contains("RazonSocialCliente"))
        {
            suscripcion.RazonSocialCliente = fila["RazonSocialCliente"].ToString();
            suscripcion.CuitCliente = fila["CuitCliente"].ToString();
            suscripcion.EmailCliente = fila["EmailCliente"].ToString();
            suscripcion.NombrePlan = fila["NombrePlan"].ToString();
            suscripcion.DescripcionPlan = fila["DescripcionPlan"].ToString();
        }

        return suscripcion;
    }

    public static List<Suscripcion> ToListaSuscripciones(this DataTable tabla)
    {
        List<Suscripcion> suscripciones = new List<Suscripcion>();

        foreach (DataRow fila in tabla.Rows)
        {
            suscripciones.Add(fila.ToSuscripcion());
        }

        return suscripciones;
    }
}

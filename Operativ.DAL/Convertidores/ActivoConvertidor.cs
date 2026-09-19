using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.DAL.Convertidores;
public static class ActivoConvertidor
{
    public static Activo ToActivo(this DataRow fila)
    {
        Activo activo = new Activo
        {
            IdActivo = (int)fila["IdActivo"],
            Nombre = fila["Nombre"].ToString(),
            Modelo = fila["Modelo"].ToString(),
            NumeroSerie = fila["NumeroSerie"].ToString(),
            Especificaciones = fila["Especificaciones"] == DBNull.Value ? null : fila["Especificaciones"].ToString(),
            IdPaquete = (int)fila["IdPaquete"],
            Estado = (EstadoActivo)Enum.Parse(typeof(EstadoActivo), fila["Estado"].ToString()),
            Habilitado = (bool)fila["Habilitado"]
        };

        if (fila.Table.Columns.Contains("NombrePaquete"))
        {
            activo.NombrePaquete = fila["NombrePaquete"].ToString();
        }

        return activo;
    }

    public static List<Activo> ToListaActivos(this DataTable tabla)
    {
        List<Activo> activos = new List<Activo>();

        foreach (DataRow fila in tabla.Rows)
        {
            activos.Add(fila.ToActivo());
        }

        return activos;
    }
}

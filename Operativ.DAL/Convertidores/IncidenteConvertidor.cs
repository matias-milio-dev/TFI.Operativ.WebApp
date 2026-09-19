using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.DAL.Convertidores;
public static class IncidenteConvertidor
{
    public static Incidente ToIncidente(this DataRow fila)
    {
        Incidente incidente = new Incidente
        {
            IdIncidente = (int)fila["IdIncidente"],
            NumeroIncidente = fila["NumeroIncidente"].ToString(),
            IdActivo = (int)fila["IdActivo"],
            Descripcion = fila["Descripcion"].ToString(),
            Categoria = (CategoriaIncidente)Enum.Parse(typeof(CategoriaIncidente), fila["Categoria"].ToString()),
            Prioridad = (PrioridadIncidente)Enum.Parse(typeof(PrioridadIncidente), fila["Prioridad"].ToString()),
            Estado = (EstadoIncidente)Enum.Parse(typeof(EstadoIncidente), fila["Estado"].ToString()),
            FechaAlta = (DateTime)fila["FechaAlta"],
            FechaCierre = fila["FechaCierre"] == DBNull.Value ? (DateTime?)null : (DateTime)fila["FechaCierre"],
            ComentarioResolucion = fila["ComentarioResolucion"] == DBNull.Value ? null : fila["ComentarioResolucion"].ToString()
        };

        if (fila.Table.Columns.Contains("NombreActivo"))
        {
            incidente.NombreActivo = fila["NombreActivo"].ToString();
            incidente.NumeroSerieActivo = fila["NumeroSerieActivo"].ToString();
            incidente.RazonSocialCliente = fila["RazonSocialCliente"].ToString();
        }

        return incidente;
    }

    public static List<Incidente> ToListaIncidentes(this DataTable tabla)
    {
        List<Incidente> incidentes = new List<Incidente>();

        foreach (DataRow fila in tabla.Rows)
        {
            incidentes.Add(fila.ToIncidente());
        }

        return incidentes;
    }
}

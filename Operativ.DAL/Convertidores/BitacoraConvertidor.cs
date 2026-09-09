using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.DAL.Convertidores;
public static class BitacoraConvertidor
{
    public static Bitacora ToBitacora(this DataRow fila)
    {
        Bitacora bitacora = new Bitacora
        {
            IdBitacora = (int)fila["IdBitacora"],
            IdUsuario = fila["IdUsuario"] == DBNull.Value ? (int?)null : (int)fila["IdUsuario"],
            FechaHora = (DateTime)fila["FechaHora"],
            Accion = (TipoAccionBitacora)Enum.Parse(typeof(TipoAccionBitacora), fila["Accion"].ToString()),
            Criticidad = (CriticidadBitacora)Enum.Parse(typeof(CriticidadBitacora), fila["Criticidad"].ToString()),
            Descripcion = fila["Descripcion"] == DBNull.Value ? null : fila["Descripcion"].ToString()
        };

        if (fila.Table.Columns.Contains("NombreUsuario") && fila["NombreUsuario"] != DBNull.Value)
        {
            bitacora.NombreUsuario = fila["NombreUsuario"].ToString();
        }

        return bitacora;
    }

    public static List<Bitacora> ToListaBitacoras(this DataTable tabla)
    {
        List<Bitacora> bitacoras = new List<Bitacora>();

        foreach (DataRow fila in tabla.Rows)
        {
            bitacoras.Add(fila.ToBitacora());
        }

        return bitacoras;
    }
}

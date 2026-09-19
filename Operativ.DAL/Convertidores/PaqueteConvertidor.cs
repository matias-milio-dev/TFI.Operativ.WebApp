using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.DAL.Convertidores;
public static class PaqueteConvertidor
{
    public static Paquete ToPaquete(this DataRow fila)
    {
        Paquete paquete = new Paquete
        {
            IdPaquete = (int)fila["IdPaquete"],
            Nombre = fila["Nombre"].ToString(),
            Descripcion = fila["Descripcion"].ToString(),
            TipoPermiso = (TipoPermiso)Enum.Parse(typeof(TipoPermiso), fila["TipoPermiso"].ToString()),
            Activo = (bool)fila["Activo"]
        };

        return paquete;
    }

    public static List<Paquete> ToListaPaquetes(this DataTable tabla)
    {
        List<Paquete> paquetes = new List<Paquete>();

        foreach (DataRow fila in tabla.Rows)
        {
            paquetes.Add(fila.ToPaquete());
        }

        return paquetes;
    }
}
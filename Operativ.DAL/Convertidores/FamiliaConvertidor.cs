using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Convertidores;
public static class FamiliaConvertidor
{
    public static Familia ToFamilia(this DataRow fila)
    {
        Familia familia = new Familia
        {
            IdFamilia = (int)fila["IdFamilia"],
            Nombre = fila["Nombre"].ToString(),
            Descripcion = fila["Descripcion"].ToString()
        };
        return familia;
    }

    public static List<Familia> ToListaFamilias(this DataTable tabla)
    {
        List<Familia> familias = new List<Familia>();

        foreach (DataRow fila in tabla.Rows)
        {
            familias.Add(fila.ToFamilia());
        }

        return familias;
    }
}

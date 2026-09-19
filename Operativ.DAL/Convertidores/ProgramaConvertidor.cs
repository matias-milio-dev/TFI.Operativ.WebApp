using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Convertidores;
public static class ProgramaConvertidor
{
    public static Programa ToPrograma(this DataRow fila)
    {
        Programa programa = new Programa
        {
            IdPrograma = (int)fila["IdPrograma"],
            Nombre = fila["Nombre"].ToString(),
            Descripcion = fila["Descripcion"].ToString()
        };

        return programa;
    }

    public static List<Programa> ToListaProgramas(this DataTable tabla)
    {
        List<Programa> programas = new List<Programa>();

        foreach (DataRow fila in tabla.Rows)
        {
            programas.Add(fila.ToPrograma());
        }

        return programas;
    }
}
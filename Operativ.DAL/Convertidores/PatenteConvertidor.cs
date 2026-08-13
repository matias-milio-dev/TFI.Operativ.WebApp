using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Convertidores
{
    public static class PatenteConvertidor
    {
        public static Patente ToPatente(this DataRow fila)
        {
            Patente patente = new Patente
            {
                IdPatente = (int)fila["IdPatente"],
                Nombre = fila["Nombre"].ToString(),
                Descripcion = fila["Descripcion"].ToString()
            };
            return patente;
        }

        public static List<Patente> ToListaPatentes(this DataTable tabla)
        {
            List<Patente> patentes = new List<Patente>();

            foreach (DataRow fila in tabla.Rows)
            {
                patentes.Add(fila.ToPatente());
            }

            return patentes;
        }
    }
}

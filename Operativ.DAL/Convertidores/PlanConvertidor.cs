using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Convertidores;
public static class PlanConvertidor
{
    public static Plan ToPlan(this DataRow fila)
    {
        Plan plan = new Plan
        {
            IdPlan = (int)fila["IdPlan"],
            Nombre = fila["Nombre"].ToString(),
            Descripcion = fila["Descripcion"].ToString(),
            PrecioAnual = (decimal)fila["PrecioAnual"],
            Activo = (bool)fila["Activo"]
        };

        return plan;
    }

    public static List<Plan> ToListaPlanes(this DataTable tabla)
    {
        List<Plan> planes = new List<Plan>();

        foreach (DataRow fila in tabla.Rows)
        {
            planes.Add(fila.ToPlan());
        }

        return planes;
    }
}

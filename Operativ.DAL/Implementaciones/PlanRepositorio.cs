using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Conexion;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;

namespace Operativ.DAL.Implementaciones;
public class PlanRepositorio : IPlanRepositorio
{
    private const string Columnas = "IdPlan, Nombre, Descripcion, PrecioAnual, Activo";

    private readonly AccesoDatos accesoDatos;

    public PlanRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Plan> ListarActivos()
    {
        string consulta = "SELECT " + Columnas + " FROM [Plan] WHERE Activo = 1 ORDER BY PrecioAnual";

        DataTable tabla = accesoDatos.EjecutarReader(consulta, null);

        return tabla.ToListaPlanes();
    }

    public Plan GetPorId(int idPlan)
    {
        string consulta = "SELECT " + Columnas + " FROM [Plan] WHERE IdPlan = @IdPlan";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdPlan", idPlan)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Plan plan = null;

        if (tabla.Rows.Count > 0)
        {
            plan = tabla.Rows[0].ToPlan();
        }

        return plan;
    }
}

using System.Data;
using System.Collections.Generic;
using Operativ.BE;

namespace Operativ.DAL
{
    public interface IPatenteDAL
    {
        List<Patente> Listar();
        List<Patente> ListarNoAsignadasAFamilia(int idFamilia);
    }

    public class PatenteDAL : IPatenteDAL
    {
        public List<Patente> Listar()
        {
            return MapearLista(DALHelper.EjecutarConsulta(
                "SELECT IdPatente, Codigo, Nombre, Descripcion, Modulo, Activo FROM dbo.Patente ORDER BY Modulo, Nombre", null));
        }

        public List<Patente> ListarNoAsignadasAFamilia(int idFamilia)
        {
            var tabla = DALHelper.EjecutarConsulta(@"
                SELECT p.IdPatente, p.Codigo, p.Nombre, p.Modulo
                FROM dbo.Patente p
                WHERE p.Activo = 1
                  AND NOT EXISTS (SELECT 1 FROM dbo.FamiliaPatente fp WHERE fp.IdFamilia = @IdFamilia AND fp.IdPatente = p.IdPatente)
                ORDER BY p.Modulo, p.Nombre",
                comando => comando.Parameters.Add("@IdFamilia", SqlDbType.Int).Value = idFamilia);
            return MapearLista(tabla);
        }

        private static List<Patente> MapearLista(DataTable tabla)
        {
            var patentes = new List<Patente>();
            foreach (DataRow fila in tabla.Rows)
            {
                patentes.Add(new Patente
                {
                    IdPatente = (int)fila["IdPatente"],
                    Codigo = (string)fila["Codigo"],
                    Nombre = (string)fila["Nombre"],
                    Modulo = (string)fila["Modulo"]
                });
            }
            return patentes;
        }
    }
}

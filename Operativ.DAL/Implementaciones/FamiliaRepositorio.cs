using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;
using Operativ.DAL.Conexion;

namespace Operativ.DAL.Implementaciones;
public class FamiliaRepositorio : IFamiliaRepositorio
{
    private readonly AccesoDatos accesoDatos;

    public FamiliaRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Familia> GetFamiliasDeUsuario(int idUsuario)
    {
        string consulta = "SELECT F.IdFamilia, F.Nombre, F.Descripcion "
            + "FROM Familia F "
            + "INNER JOIN UsuarioFamilia UF ON UF.IdFamilia = F.IdFamilia "
            + "WHERE UF.IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaFamilias();
    }

    public List<Patente> GetPatentesDeFamilia(int idFamilia)
    {
        string consulta = "SELECT P.IdPatente, P.Nombre, P.Descripcion "
            + "FROM Patente P "
            + "INNER JOIN FamiliaPatente FP ON FP.IdPatente = P.IdPatente "
            + "WHERE FP.IdFamilia = @IdFamilia";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdFamilia", idFamilia)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaPatentes();
    }

    public List<Familia> ListarTodas()
    {
        string consulta = "SELECT IdFamilia, Nombre, Descripcion FROM Familia ORDER BY Nombre";

        DataTable tabla = accesoDatos.EjecutarReader(consulta, null);

        return tabla.ToListaFamilias();
    }

    public Familia GetPorId(int idFamilia)
    {
        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdFamilia", idFamilia)
        };

        return GetPrimeraFamilia("WHERE IdFamilia = @IdFamilia", parametros);
    }

    public Familia GetPorNombre(string nombre)
    {
        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Nombre", nombre)
        };

        return GetPrimeraFamilia("WHERE Nombre = @Nombre", parametros);
    }

    private Familia GetPrimeraFamilia(string filtro, List<SqlParameter> parametros)
    {
        string consulta = "SELECT IdFamilia, Nombre, Descripcion FROM Familia " + filtro;

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Familia familia = null;

        if (tabla.Rows.Count > 0)
        {
            familia = tabla.Rows[0].ToFamilia();
        }

        return familia;
    }
}

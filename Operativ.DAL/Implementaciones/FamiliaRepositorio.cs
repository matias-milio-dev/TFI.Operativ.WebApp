using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;
using Operativ.DAL.Conexion;
using Operativ.DAL.Integridad;

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

    public void AsignarPatentesAFamilia(int idFamilia, int[] idsPatente)
    {
        if (idsPatente.Length == 0)
        {
            return;
        }

        StringBuilder filas = new StringBuilder();

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdFamilia", idFamilia)
        };

        for (int posicion = 0; posicion < idsPatente.Length; posicion++)
        {
            if (posicion > 0)
            {
                filas.Append(", ");
            }

            filas.Append("(@IdFamilia, @IdPatente").Append(posicion).Append(")");
            parametros.Add(new SqlParameter("@IdPatente" + posicion, idsPatente[posicion]));
        }

        string consulta = "INSERT INTO FamiliaPatente (IdFamilia, IdPatente) VALUES " + filas.ToString();

        accesoDatos.EjecutarConsulta(consulta, parametros);

        ActualizarIntegridadDeFilas(idFamilia, idsPatente);
    }

    public void QuitarPatentesDeFamilia(int idFamilia, int[] idsPatente)
    {
        if (idsPatente.Length == 0)
        {
            return;
        }

        StringBuilder marcadores = new StringBuilder();

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdFamilia", idFamilia)
        };

        for (int posicion = 0; posicion < idsPatente.Length; posicion++)
        {
            if (posicion > 0)
            {
                marcadores.Append(", ");
            }

            marcadores.Append("@IdPatente").Append(posicion);
            parametros.Add(new SqlParameter("@IdPatente" + posicion, idsPatente[posicion]));
        }

        string consulta = "DELETE FROM FamiliaPatente WHERE IdFamilia = @IdFamilia AND IdPatente IN (" + marcadores.ToString() + ")";

        accesoDatos.EjecutarConsulta(consulta, parametros);

        IntegridadHelper.ActualizarDvvTabla("FamiliaPatente");
    }

    private void ActualizarIntegridadDeFilas(int idFamilia, int[] idsPatente)
    {
        List<SqlParameter>[] clavesFilas = new List<SqlParameter>[idsPatente.Length];

        for (int posicion = 0; posicion < idsPatente.Length; posicion++)
        {
            clavesFilas[posicion] = new List<SqlParameter>
            {
                new SqlParameter("@IdFamilia", idFamilia),
                new SqlParameter("@IdPatente", idsPatente[posicion])
            };
        }

        IntegridadHelper.ActualizarIntegridadClaveCompuestaEnLote("FamiliaPatente", clavesFilas);
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

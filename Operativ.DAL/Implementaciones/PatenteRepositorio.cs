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
public class PatenteRepositorio : IPatenteRepositorio
{
    private readonly AccesoDatos accesoDatos;

    public PatenteRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Patente> ListarTodas()
    {
        string consulta = "SELECT IdPatente, Nombre, Descripcion FROM Patente ORDER BY Nombre";

        DataTable tabla = accesoDatos.EjecutarReader(consulta, null);

        return tabla.ToListaPatentes();
    }

    public List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario)
    {
        string consulta = "SELECT P.IdPatente, P.Nombre, P.Descripcion "
            + "FROM Patente P "
            + "INNER JOIN UsuarioPatente UP ON UP.IdPatente = P.IdPatente "
            + "WHERE UP.IdUsuario = @IdUsuario "
            + "ORDER BY P.Nombre";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaPatentes();
    }

    public void AsignarPatentesAUsuario(int idUsuario, int[] idsPatente)
    {
        if (idsPatente.Length == 0)
        {
            return;
        }

        StringBuilder filas = new StringBuilder();

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        for (int posicion = 0; posicion < idsPatente.Length; posicion++)
        {
            if (posicion > 0)
            {
                filas.Append(", ");
            }

            filas.Append("(@IdUsuario, @IdPatente").Append(posicion).Append(")");
            parametros.Add(new SqlParameter("@IdPatente" + posicion, idsPatente[posicion]));
        }

        string consulta = "INSERT INTO UsuarioPatente (IdUsuario, IdPatente) VALUES " + filas.ToString();

        accesoDatos.EjecutarConsulta(consulta, parametros);

        ActualizarIntegridadDeFilas(idUsuario, idsPatente);
    }

    public void QuitarPatentesDeUsuario(int idUsuario, int[] idsPatente)
    {
        if (idsPatente.Length == 0)
        {
            return;
        }

        StringBuilder marcadores = new StringBuilder();

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
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

        string consulta = "DELETE FROM UsuarioPatente WHERE IdUsuario = @IdUsuario AND IdPatente IN (" + marcadores.ToString() + ")";

        accesoDatos.EjecutarConsulta(consulta, parametros);

        IntegridadHelper.ActualizarDvvTabla("UsuarioPatente");
    }

    private void ActualizarIntegridadDeFilas(int idUsuario, int[] idsPatente)
    {
        List<SqlParameter>[] clavesFilas = new List<SqlParameter>[idsPatente.Length];

        for (int posicion = 0; posicion < idsPatente.Length; posicion++)
        {
            clavesFilas[posicion] = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", idUsuario),
                new SqlParameter("@IdPatente", idsPatente[posicion])
            };
        }

        IntegridadHelper.ActualizarIntegridadClaveCompuestaEnLote("UsuarioPatente", clavesFilas);
    }
}

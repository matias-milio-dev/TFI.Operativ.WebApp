using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

    public void AsignarPatenteAUsuario(int idUsuario, int idPatente)
    {
        string consulta = "INSERT INTO UsuarioPatente (IdUsuario, IdPatente) VALUES (@IdUsuario, @IdPatente)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdPatente", idPatente)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);

        List<SqlParameter> clavesFila = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdPatente", idPatente)
        };

        IntegridadHelper.ActualizarIntegridadClaveCompuesta("UsuarioPatente", clavesFila);
    }

    public void QuitarPatenteDeUsuario(int idUsuario, int idPatente)
    {
        string consulta = "DELETE FROM UsuarioPatente WHERE IdUsuario = @IdUsuario AND IdPatente = @IdPatente";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdPatente", idPatente)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        IntegridadHelper.ActualizarDvvTabla("UsuarioPatente");
    }

    public bool ExistePatenteIndividual(int idUsuario, int idPatente)
    {
        string consulta = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdUsuario = @IdUsuario AND IdPatente = @IdPatente";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdPatente", idPatente)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado) > 0;
    }
}

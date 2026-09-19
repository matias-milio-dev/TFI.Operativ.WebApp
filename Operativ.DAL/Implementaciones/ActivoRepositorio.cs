using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Conexion;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;
using Operativ.DAL.Integridad;

namespace Operativ.DAL.Implementaciones;
public class ActivoRepositorio : IActivoRepositorio, IVerificable
{
    private readonly AccesoDatos accesoDatos;

    public ActivoRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Activo> Listar(string filtro, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT A.IdActivo, A.Nombre, A.Modelo, A.NumeroSerie, A.Especificaciones, A.IdPaquete, A.Estado, A.Habilitado, P.Nombre AS NombrePaquete "
            + "FROM Activo A "
            + "INNER JOIN Paquete P ON P.IdPaquete = A.IdPaquete "
            + "WHERE A.Habilitado = 1 "
            + "AND (@Filtro = '' OR A.Nombre LIKE '%' + @Filtro + '%' OR A.Modelo LIKE '%' + @Filtro + '%' OR A.NumeroSerie LIKE '%' + @Filtro + '%') "
            + "ORDER BY A.Nombre "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty),
            new SqlParameter("@Salteo", salteo),
            new SqlParameter("@TamanioPagina", tamanioPagina)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaActivos();
    }

    public int ContarActivos(string filtro)
    {
        string consulta = "SELECT COUNT(*) FROM Activo "
            + "WHERE Habilitado = 1 "
            + "AND (@Filtro = '' OR Nombre LIKE '%' + @Filtro + '%' OR Modelo LIKE '%' + @Filtro + '%' OR NumeroSerie LIKE '%' + @Filtro + '%')";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public Activo GetPorId(int idActivo)
    {
        string consulta = "SELECT IdActivo, Nombre, Modelo, NumeroSerie, Especificaciones, IdPaquete, Estado, Habilitado "
            + "FROM Activo WHERE IdActivo = @IdActivo";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdActivo", idActivo)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Activo activo = null;

        if (tabla.Rows.Count > 0)
        {
            activo = tabla.Rows[0].ToActivo();
        }

        return activo;
    }

    public int Insertar(Activo activo)
    {
        string consulta = "INSERT INTO Activo (Nombre, Modelo, NumeroSerie, Especificaciones, IdPaquete, Estado, Habilitado) "
            + "VALUES (@Nombre, @Modelo, @NumeroSerie, @Especificaciones, @IdPaquete, @Estado, 1); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        List<SqlParameter> parametros = ArmarParametrosDatos(activo);

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idActivo = Convert.ToInt32(resultado);
        ActualizarDVH(idActivo);
        return idActivo;
    }

    public void Modificar(Activo activo)
    {
        string consulta = "UPDATE Activo SET Nombre = @Nombre, Modelo = @Modelo, NumeroSerie = @NumeroSerie, "
            + "Especificaciones = @Especificaciones, IdPaquete = @IdPaquete, Estado = @Estado "
            + "WHERE IdActivo = @IdActivo";

        List<SqlParameter> parametros = ArmarParametrosDatos(activo);
        parametros.Add(new SqlParameter("@IdActivo", activo.IdActivo));

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(activo.IdActivo);
    }

    public void BajaLogica(int idActivo)
    {
        string consulta = "UPDATE Activo SET Habilitado = 0 WHERE IdActivo = @IdActivo";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdActivo", idActivo)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idActivo);
    }

    public bool ExisteNumeroSerie(string numeroSerie, int? idActivoExcluir)
    {
        string consulta = "SELECT COUNT(*) FROM Activo "
            + "WHERE NumeroSerie = @NumeroSerie "
            + "AND (@IdActivoExcluir IS NULL OR IdActivo <> @IdActivoExcluir)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NumeroSerie", numeroSerie),
            new SqlParameter("@IdActivoExcluir", (object)idActivoExcluir ?? DBNull.Value)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado) > 0;
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Activo", "IdActivo", id);
    }

    private List<SqlParameter> ArmarParametrosDatos(Activo activo)
    {
        object especificaciones = string.IsNullOrEmpty(activo.Especificaciones) ? (object)DBNull.Value : activo.Especificaciones;

        return new List<SqlParameter>
        {
            new SqlParameter("@Nombre", activo.Nombre),
            new SqlParameter("@Modelo", activo.Modelo),
            new SqlParameter("@NumeroSerie", activo.NumeroSerie),
            new SqlParameter("@Especificaciones", especificaciones),
            new SqlParameter("@IdPaquete", activo.IdPaquete),
            new SqlParameter("@Estado", activo.Estado.ToString())
        };
    }
}

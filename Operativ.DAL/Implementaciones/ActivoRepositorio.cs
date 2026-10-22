using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
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

    public List<Activo> Listar(string filtro, int? idCliente, EstadoActivo? estado, int? idPaquete, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT A.IdActivo, A.Nombre, A.Modelo, A.NumeroSerie, A.Especificaciones, A.IdPaquete, A.IdCliente, A.Estado, A.Habilitado, "
            + "P.Nombre AS NombrePaquete, C.RazonSocial AS RazonSocialCliente "
            + "FROM Activo A "
            + "INNER JOIN Paquete P ON P.IdPaquete = A.IdPaquete "
            + "INNER JOIN Cliente C ON C.IdCliente = A.IdCliente "
            + "WHERE A.Habilitado = 1 "
            + "AND (@IdCliente IS NULL OR A.IdCliente = @IdCliente) "
            + "AND (@Estado IS NULL OR A.Estado = @Estado) "
            + "AND (@IdPaquete IS NULL OR A.IdPaquete = @IdPaquete) "
            + "AND (@Filtro = '' OR A.Nombre LIKE '%' + @Filtro + '%' OR A.Modelo LIKE '%' + @Filtro + '%' OR A.NumeroSerie LIKE '%' + @Filtro + '%') "
            + "ORDER BY A.Nombre "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty),
            new SqlParameter("@IdCliente", (object)idCliente ?? DBNull.Value),
            new SqlParameter("@Estado", estado.HasValue ? (object)estado.Value.ToString() : DBNull.Value),
            new SqlParameter("@IdPaquete", (object)idPaquete ?? DBNull.Value),
            new SqlParameter("@Salteo", salteo),
            new SqlParameter("@TamanioPagina", tamanioPagina)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaActivos();
    }

    public int ContarActivos(string filtro, int? idCliente, EstadoActivo? estado, int? idPaquete)
    {
        string consulta = "SELECT COUNT(*) FROM Activo "
            + "WHERE Habilitado = 1 "
            + "AND (@IdCliente IS NULL OR IdCliente = @IdCliente) "
            + "AND (@Estado IS NULL OR Estado = @Estado) "
            + "AND (@IdPaquete IS NULL OR IdPaquete = @IdPaquete) "
            + "AND (@Filtro = '' OR Nombre LIKE '%' + @Filtro + '%' OR Modelo LIKE '%' + @Filtro + '%' OR NumeroSerie LIKE '%' + @Filtro + '%')";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty),
            new SqlParameter("@IdCliente", (object)idCliente ?? DBNull.Value),
            new SqlParameter("@Estado", estado.HasValue ? (object)estado.Value.ToString() : DBNull.Value),
            new SqlParameter("@IdPaquete", (object)idPaquete ?? DBNull.Value)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public ResumenActivos ObtenerResumen(int? idCliente)
    {
        // SUM devuelve NULL (no 0) cuando no hay filas, por eso el ISNULL.
        string consulta = "SELECT COUNT(*) AS Total, "
            + "ISNULL(SUM(CASE WHEN Estado = @EstadoDisponible THEN 1 ELSE 0 END), 0) AS Disponibles, "
            + "ISNULL(SUM(CASE WHEN Estado = @EstadoAsignado THEN 1 ELSE 0 END), 0) AS Asignados, "
            + "ISNULL(SUM(CASE WHEN Estado = @EstadoEnReparacion THEN 1 ELSE 0 END), 0) AS EnReparacion "
            + "FROM Activo "
            + "WHERE Habilitado = 1 "
            + "AND (@IdCliente IS NULL OR IdCliente = @IdCliente)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdCliente", (object)idCliente ?? DBNull.Value),
            new SqlParameter("@EstadoDisponible", EstadoActivo.Disponible.ToString()),
            new SqlParameter("@EstadoAsignado", EstadoActivo.Asignado.ToString()),
            new SqlParameter("@EstadoEnReparacion", EstadoActivo.EnReparacion.ToString())
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);
        DataRow fila = tabla.Rows[0];

        return new ResumenActivos
        {
            Total = Convert.ToInt32(fila["Total"]),
            Disponibles = Convert.ToInt32(fila["Disponibles"]),
            Asignados = Convert.ToInt32(fila["Asignados"]),
            EnReparacion = Convert.ToInt32(fila["EnReparacion"])
        };
    }

    public Activo GetPorId(int idActivo)
    {
        string consulta = "SELECT IdActivo, Nombre, Modelo, NumeroSerie, Especificaciones, IdPaquete, IdCliente, Estado, Habilitado "
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

    public List<Activo> ListarPorCliente(int idCliente)
    {
        string consulta = "SELECT A.IdActivo, A.Nombre, A.Modelo, A.NumeroSerie, A.Especificaciones, A.IdPaquete, A.IdCliente, A.Estado, A.Habilitado, "
            + "P.Nombre AS NombrePaquete, C.RazonSocial AS RazonSocialCliente "
            + "FROM Activo A "
            + "INNER JOIN Paquete P ON P.IdPaquete = A.IdPaquete "
            + "INNER JOIN Cliente C ON C.IdCliente = A.IdCliente "
            + "WHERE A.Habilitado = 1 AND A.IdCliente = @IdCliente "
            + "ORDER BY A.Nombre";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdCliente", idCliente)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaActivos();
    }

    public int Insertar(Activo activo)
    {
        string consulta = "INSERT INTO Activo (Nombre, Modelo, NumeroSerie, Especificaciones, IdPaquete, IdCliente, Estado, Habilitado) "
            + "VALUES (@Nombre, @Modelo, @NumeroSerie, @Especificaciones, @IdPaquete, @IdCliente, @Estado, 1); "
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
            + "Especificaciones = @Especificaciones, IdPaquete = @IdPaquete, IdCliente = @IdCliente, Estado = @Estado "
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
            new SqlParameter("@IdCliente", activo.IdCliente),
            new SqlParameter("@Estado", activo.Estado.ToString())
        };
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.DAL.Conexion;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;
using Operativ.DAL.Integridad;

namespace Operativ.DAL.Implementaciones;
public class SuscripcionRepositorio : ISuscripcionRepositorio, IVerificable
{
    private const string ColumnasConJoin = "S.IdSuscripcion, S.IdCliente, S.IdPlan, S.PrecioAnual, S.Estado, S.FechaAlta, S.FechaFinTrial, "
        + "S.FechaPago, S.FechaVencimiento, S.FechaCancelacion, S.MedioPago, S.CodigoComprobante, "
        + "C.RazonSocial AS RazonSocialCliente, C.Cuit AS CuitCliente, C.Email AS EmailCliente, "
        + "P.Nombre AS NombrePlan, P.Descripcion AS DescripcionPlan";

    private const string DesdeConJoin = "FROM Suscripcion S "
        + "INNER JOIN Cliente C ON C.IdCliente = S.IdCliente "
        + "INNER JOIN [Plan] P ON P.IdPlan = S.IdPlan ";

    private const string Filtros = "WHERE (@IdCliente IS NULL OR S.IdCliente = @IdCliente) "
        + "AND (@Filtro = '' OR C.RazonSocial LIKE '%' + @Filtro + '%' OR C.Cuit LIKE '%' + @Filtro + '%' OR P.Nombre LIKE '%' + @Filtro + '%') ";

    private readonly AccesoDatos accesoDatos;

    public SuscripcionRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Suscripcion> Listar(string filtro, int? idCliente, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT " + ColumnasConJoin + " " + DesdeConJoin + Filtros
            + "ORDER BY S.FechaAlta DESC, S.IdSuscripcion DESC "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = ArmarParametrosFiltro(filtro, idCliente);
        parametros.Add(new SqlParameter("@Salteo", salteo));
        parametros.Add(new SqlParameter("@TamanioPagina", tamanioPagina));

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaSuscripciones();
    }

    public int ContarSuscripciones(string filtro, int? idCliente)
    {
        string consulta = "SELECT COUNT(*) " + DesdeConJoin + Filtros;

        List<SqlParameter> parametros = ArmarParametrosFiltro(filtro, idCliente);

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public Suscripcion GetPorId(int idSuscripcion)
    {
        string consulta = "SELECT " + ColumnasConJoin + " " + DesdeConJoin + "WHERE S.IdSuscripcion = @IdSuscripcion";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdSuscripcion", idSuscripcion)
        };

        return ObtenerPrimera(consulta, parametros);
    }

    public Suscripcion GetVigentePorCliente(int idCliente)
    {
        string consulta = "SELECT TOP 1 " + ColumnasConJoin + " " + DesdeConJoin
            + "WHERE S.IdCliente = @IdCliente AND S.Estado IN (@EstadoActiva, @EstadoPendiente) "
            + "ORDER BY S.FechaAlta DESC, S.IdSuscripcion DESC";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdCliente", idCliente),
            new SqlParameter("@EstadoActiva", EstadoSuscripcion.Activa.ToString()),
            new SqlParameter("@EstadoPendiente", EstadoSuscripcion.PendientePago.ToString())
        };

        return ObtenerPrimera(consulta, parametros);
    }

    public int Insertar(Suscripcion suscripcion)
    {
        string consulta = "INSERT INTO Suscripcion (IdCliente, IdPlan, PrecioAnual, Estado, FechaAlta, FechaFinTrial) "
            + "VALUES (@IdCliente, @IdPlan, @PrecioAnual, @Estado, GETDATE(), @FechaFinTrial); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdCliente", suscripcion.IdCliente),
            new SqlParameter("@IdPlan", suscripcion.IdPlan),
            new SqlParameter("@PrecioAnual", suscripcion.PrecioAnual),
            new SqlParameter("@Estado", suscripcion.Estado.ToString()),
            new SqlParameter("@FechaFinTrial", suscripcion.FechaFinTrial)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idSuscripcion = Convert.ToInt32(resultado);
        ActualizarDVH(idSuscripcion);
        return idSuscripcion;
    }

    public void Cancelar(int idSuscripcion)
    {
        string consulta = "UPDATE Suscripcion SET Estado = @Estado, FechaCancelacion = GETDATE() WHERE IdSuscripcion = @IdSuscripcion";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Estado", EstadoSuscripcion.Cancelada.ToString()),
            new SqlParameter("@IdSuscripcion", idSuscripcion)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idSuscripcion);
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Suscripcion", "IdSuscripcion", id);
    }

    private List<SqlParameter> ArmarParametrosFiltro(string filtro, int? idCliente)
    {
        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty),
            new SqlParameter("@IdCliente", (object)idCliente ?? DBNull.Value)
        };

        return parametros;
    }

    private Suscripcion ObtenerPrimera(string consulta, List<SqlParameter> parametros)
    {
        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Suscripcion suscripcion = null;

        if (tabla.Rows.Count > 0)
        {
            suscripcion = tabla.Rows[0].ToSuscripcion();
        }

        return suscripcion;
    }
}

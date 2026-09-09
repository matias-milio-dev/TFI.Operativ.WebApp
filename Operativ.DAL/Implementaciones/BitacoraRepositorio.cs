using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;
using Operativ.DAL.Conexion;
using Operativ.DAL.Integridad;

namespace Operativ.DAL.Implementaciones;
public class BitacoraRepositorio : IBitacoraRepositorio, IVerificable
{
    private readonly AccesoDatos accesoDatos;

    public BitacoraRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public void Registrar(Bitacora entrada)
    {
        string consulta = "INSERT INTO Bitacora (IdUsuario, Accion, Criticidad, Descripcion) "
            + "VALUES (@IdUsuario, @Accion, @Criticidad, @Descripcion); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        object descripcion = entrada.Descripcion ?? (object)DBNull.Value;
        object idUsuario = entrada.IdUsuario.HasValue ? (object)entrada.IdUsuario.Value : DBNull.Value;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@Accion", entrada.Accion.ToString()),
            new SqlParameter("@Criticidad", entrada.Criticidad.ToString()),
            new SqlParameter("@Descripcion", descripcion)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idBitacora = Convert.ToInt32(resultado);
        ActualizarDVH(idBitacora);
    }

    public List<Bitacora> Buscar(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT B.IdBitacora, B.IdUsuario, B.FechaHora, B.Accion, B.Criticidad, B.Descripcion, U.NombreUsuario "
            + "FROM Bitacora B "
            + "LEFT JOIN Usuario U ON U.IdUsuario = B.IdUsuario "
            + "WHERE (@FiltroUsuario = '' OR U.NombreUsuario LIKE '%' + @FiltroUsuario + '%') "
            + (accion.HasValue ? "AND B.Accion = @Accion " : string.Empty)
            + (criticidad.HasValue ? "AND B.Criticidad = @Criticidad " : string.Empty)
            + (fechaDesde.HasValue ? "AND B.FechaHora >= @FechaDesde " : string.Empty)
            + (fechaHasta.HasValue ? "AND B.FechaHora < @FechaHasta " : string.Empty)
            + "ORDER BY B.FechaHora DESC "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@FiltroUsuario", filtroUsuario ?? string.Empty),
            new SqlParameter("@Salteo", salteo),
            new SqlParameter("@TamanioPagina", tamanioPagina)
        };

        if (accion.HasValue)
        {
            parametros.Add(new SqlParameter("@Accion", accion.Value.ToString()));
        }

        if (criticidad.HasValue)
        {
            parametros.Add(new SqlParameter("@Criticidad", criticidad.Value.ToString()));
        }

        if (fechaDesde.HasValue)
        {
            parametros.Add(new SqlParameter("@FechaDesde", fechaDesde.Value.Date));
        }

        if (fechaHasta.HasValue)
        {
            parametros.Add(new SqlParameter("@FechaHasta", fechaHasta.Value.Date.AddDays(1)));
        }

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaBitacoras();
    }

    public int ContarRegistros(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        string consulta = "SELECT COUNT(*) FROM Bitacora B "
            + "LEFT JOIN Usuario U ON U.IdUsuario = B.IdUsuario "
            + "WHERE (@FiltroUsuario = '' OR U.NombreUsuario LIKE '%' + @FiltroUsuario + '%') "
            + (accion.HasValue ? "AND B.Accion = @Accion " : string.Empty)
            + (criticidad.HasValue ? "AND B.Criticidad = @Criticidad " : string.Empty)
            + (fechaDesde.HasValue ? "AND B.FechaHora >= @FechaDesde " : string.Empty)
            + (fechaHasta.HasValue ? "AND B.FechaHora < @FechaHasta " : string.Empty);

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@FiltroUsuario", filtroUsuario ?? string.Empty)
        };

        if (accion.HasValue)
        {
            parametros.Add(new SqlParameter("@Accion", accion.Value.ToString()));
        }

        if (criticidad.HasValue)
        {
            parametros.Add(new SqlParameter("@Criticidad", criticidad.Value.ToString()));
        }

        if (fechaDesde.HasValue)
        {
            parametros.Add(new SqlParameter("@FechaDesde", fechaDesde.Value.Date));
        }

        if (fechaHasta.HasValue)
        {
            parametros.Add(new SqlParameter("@FechaHasta", fechaHasta.Value.Date.AddDays(1)));
        }

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Bitacora", "IdBitacora", id);
    }
}

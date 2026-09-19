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
public class IncidenteRepositorio : IIncidenteRepositorio, IVerificable
{
    private const string ColumnasConJoin = "I.IdIncidente, I.NumeroIncidente, I.IdActivo, I.Descripcion, I.Categoria, I.Prioridad, I.Estado, "
        + "I.FechaAlta, I.FechaCierre, I.ComentarioResolucion, "
        + "A.Nombre AS NombreActivo, A.NumeroSerie AS NumeroSerieActivo, C.RazonSocial AS RazonSocialCliente";

    private const string DesdeConJoin = "FROM Incidente I "
        + "INNER JOIN Activo A ON A.IdActivo = I.IdActivo "
        + "INNER JOIN Cliente C ON C.IdCliente = A.IdCliente ";

    private readonly AccesoDatos accesoDatos;

    public IncidenteRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Incidente> Listar(string filtro, EstadoIncidente? estado, int? idCliente, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT " + ColumnasConJoin + " " + DesdeConJoin
            + ArmarFiltros(estado)
            + "ORDER BY I.FechaAlta DESC, I.IdIncidente DESC "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = ArmarParametrosFiltro(filtro, estado, idCliente);
        parametros.Add(new SqlParameter("@Salteo", salteo));
        parametros.Add(new SqlParameter("@TamanioPagina", tamanioPagina));

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaIncidentes();
    }

    public int ContarIncidentes(string filtro, EstadoIncidente? estado, int? idCliente)
    {
        string consulta = "SELECT COUNT(*) " + DesdeConJoin + ArmarFiltros(estado);

        List<SqlParameter> parametros = ArmarParametrosFiltro(filtro, estado, idCliente);

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public Incidente GetPorId(int idIncidente)
    {
        string consulta = "SELECT " + ColumnasConJoin + " " + DesdeConJoin + "WHERE I.IdIncidente = @IdIncidente";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdIncidente", idIncidente)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Incidente incidente = null;

        if (tabla.Rows.Count > 0)
        {
            incidente = tabla.Rows[0].ToIncidente();
        }

        return incidente;
    }

    public int ContarIncidentesDelAnio(int anio)
    {
        string consulta = "SELECT COUNT(*) FROM Incidente WHERE YEAR(FechaAlta) = @Anio";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Anio", anio)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public int Insertar(Incidente incidente)
    {
        string consulta = "INSERT INTO Incidente (NumeroIncidente, IdActivo, Descripcion, Categoria, Prioridad, Estado, FechaAlta) "
            + "VALUES (@NumeroIncidente, @IdActivo, @Descripcion, @Categoria, @Prioridad, @Estado, GETDATE()); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NumeroIncidente", incidente.NumeroIncidente),
            new SqlParameter("@IdActivo", incidente.IdActivo),
            new SqlParameter("@Descripcion", incidente.Descripcion),
            new SqlParameter("@Categoria", incidente.Categoria.ToString()),
            new SqlParameter("@Prioridad", incidente.Prioridad.ToString()),
            new SqlParameter("@Estado", incidente.Estado.ToString())
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idIncidente = Convert.ToInt32(resultado);
        ActualizarDVH(idIncidente);
        return idIncidente;
    }

    public void Cerrar(int idIncidente, string comentarioResolucion)
    {
        string consulta = "UPDATE Incidente SET Estado = @Estado, FechaCierre = GETDATE(), ComentarioResolucion = @ComentarioResolucion "
            + "WHERE IdIncidente = @IdIncidente";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Estado", EstadoIncidente.Cerrado.ToString()),
            new SqlParameter("@ComentarioResolucion", comentarioResolucion),
            new SqlParameter("@IdIncidente", idIncidente)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idIncidente);
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Incidente", "IdIncidente", id);
    }

    private string ArmarFiltros(EstadoIncidente? estado)
    {
        return "WHERE (@IdCliente IS NULL OR A.IdCliente = @IdCliente) "
            + "AND (@Filtro = '' OR I.NumeroIncidente LIKE '%' + @Filtro + '%' OR I.Descripcion LIKE '%' + @Filtro + '%' OR A.NumeroSerie LIKE '%' + @Filtro + '%') "
            + (estado.HasValue ? "AND I.Estado = @Estado " : string.Empty);
    }

    private List<SqlParameter> ArmarParametrosFiltro(string filtro, EstadoIncidente? estado, int? idCliente)
    {
        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty),
            new SqlParameter("@IdCliente", (object)idCliente ?? DBNull.Value)
        };

        if (estado.HasValue)
        {
            parametros.Add(new SqlParameter("@Estado", estado.Value.ToString()));
        }

        return parametros;
    }
}

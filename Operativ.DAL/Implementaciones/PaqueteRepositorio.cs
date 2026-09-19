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
public class PaqueteRepositorio : IPaqueteRepositorio, IVerificable
{
    private readonly AccesoDatos accesoDatos;

    public PaqueteRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Paquete> Listar(string filtro, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT IdPaquete, Nombre, Descripcion, TipoPermiso, Activo "
            + "FROM Paquete "
            + "WHERE Activo = 1 "
            + "AND (@Filtro = '' OR Nombre LIKE '%' + @Filtro + '%' OR Descripcion LIKE '%' + @Filtro + '%') "
            + "ORDER BY Nombre "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty),
            new SqlParameter("@Salteo", salteo),
            new SqlParameter("@TamanioPagina", tamanioPagina)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaPaquetes();
    }

    public int ContarPaquetes(string filtro)
    {
        string consulta = "SELECT COUNT(*) FROM Paquete "
            + "WHERE Activo = 1 "
            + "AND (@Filtro = '' OR Nombre LIKE '%' + @Filtro + '%' OR Descripcion LIKE '%' + @Filtro + '%')";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public Paquete GetPorId(int idPaquete)
    {
        string consulta = "SELECT IdPaquete, Nombre, Descripcion, TipoPermiso, Activo FROM Paquete WHERE IdPaquete = @IdPaquete";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdPaquete", idPaquete)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Paquete paquete = null;

        if (tabla.Rows.Count > 0)
        {
            paquete = tabla.Rows[0].ToPaquete();
        }

        return paquete;
    }

    public List<Programa> GetProgramasDePaquete(int idPaquete)
    {
        string consulta = "SELECT PR.IdPrograma, PR.Nombre, PR.Descripcion "
            + "FROM Programa PR "
            + "INNER JOIN PaquetePrograma PP ON PP.IdPrograma = PR.IdPrograma "
            + "WHERE PP.IdPaquete = @IdPaquete "
            + "ORDER BY PR.Nombre";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdPaquete", idPaquete)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaProgramas();
    }

    public int Insertar(Paquete paquete)
    {
        string consulta = "INSERT INTO Paquete (Nombre, Descripcion, TipoPermiso, Activo) "
            + "VALUES (@Nombre, @Descripcion, @TipoPermiso, 1); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Nombre", paquete.Nombre),
            new SqlParameter("@Descripcion", paquete.Descripcion),
            new SqlParameter("@TipoPermiso", paquete.TipoPermiso.ToString())
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idPaquete = Convert.ToInt32(resultado);
        ActualizarDVH(idPaquete);
        return idPaquete;
    }

    public void Modificar(Paquete paquete)
    {
        string consulta = "UPDATE Paquete SET Nombre = @Nombre, Descripcion = @Descripcion, TipoPermiso = @TipoPermiso WHERE IdPaquete = @IdPaquete";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Nombre", paquete.Nombre),
            new SqlParameter("@Descripcion", paquete.Descripcion),
            new SqlParameter("@TipoPermiso", paquete.TipoPermiso.ToString()),
            new SqlParameter("@IdPaquete", paquete.IdPaquete)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(paquete.IdPaquete);
    }

    public void BajaLogica(int idPaquete)
    {
        string consulta = "UPDATE Paquete SET Activo = 0 WHERE IdPaquete = @IdPaquete";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdPaquete", idPaquete)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idPaquete);
    }

    public void AsignarProgramas(int idPaquete, int[] idsPrograma)
    {
        foreach (int idPrograma in idsPrograma)
        {
            string consulta = "INSERT INTO PaquetePrograma (IdPaquete, IdPrograma) VALUES (@IdPaquete, @IdPrograma)";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@IdPaquete", idPaquete),
                new SqlParameter("@IdPrograma", idPrograma)
            };

            accesoDatos.EjecutarConsulta(consulta, parametros);

            List<SqlParameter> clavesFila = new List<SqlParameter>
            {
                new SqlParameter("@IdPaquete", idPaquete),
                new SqlParameter("@IdPrograma", idPrograma)
            };

            IntegridadHelper.ActualizarIntegridadClaveCompuesta("PaquetePrograma", clavesFila);
        }
    }

    public void QuitarProgramas(int idPaquete, int[] idsPrograma)
    {
        if (idsPrograma.Length == 0)
        {
            return;
        }

        foreach (int idPrograma in idsPrograma)
        {
            string consulta = "DELETE FROM PaquetePrograma WHERE IdPaquete = @IdPaquete AND IdPrograma = @IdPrograma";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@IdPaquete", idPaquete),
                new SqlParameter("@IdPrograma", idPrograma)
            };

            accesoDatos.EjecutarConsulta(consulta, parametros);
        }

        IntegridadHelper.ActualizarDvvTabla("PaquetePrograma");
    }

    public bool ExisteNombre(string nombre, int? idPaqueteExcluir)
    {
        string consulta = "SELECT COUNT(*) FROM Paquete "
            + "WHERE Nombre = @Nombre "
            + "AND (@IdPaqueteExcluir IS NULL OR IdPaquete <> @IdPaqueteExcluir)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Nombre", nombre),
            new SqlParameter("@IdPaqueteExcluir", (object)idPaqueteExcluir ?? DBNull.Value)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado) > 0;
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Paquete", "IdPaquete", id);
    }
}
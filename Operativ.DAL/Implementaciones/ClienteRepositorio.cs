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
public class ClienteRepositorio : IClienteRepositorio, IVerificable
{
    private readonly AccesoDatos accesoDatos;

    public ClienteRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Cliente> Listar(string filtro, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT IdCliente, RazonSocial, Cuit, Email, Activo "
            + "FROM Cliente "
            + "WHERE Activo = 1 "
            + "AND (@Filtro = '' OR RazonSocial LIKE '%' + @Filtro + '%' OR Cuit LIKE '%' + @Filtro + '%' OR Email LIKE '%' + @Filtro + '%') "
            + "ORDER BY RazonSocial "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty),
            new SqlParameter("@Salteo", salteo),
            new SqlParameter("@TamanioPagina", tamanioPagina)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaClientes();
    }

    public int ContarClientes(string filtro)
    {
        string consulta = "SELECT COUNT(*) FROM Cliente "
            + "WHERE Activo = 1 "
            + "AND (@Filtro = '' OR RazonSocial LIKE '%' + @Filtro + '%' OR Cuit LIKE '%' + @Filtro + '%' OR Email LIKE '%' + @Filtro + '%')";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public List<Cliente> ListarActivos(int? idClienteIncluir)
    {
        string consulta = "SELECT IdCliente, RazonSocial, Cuit, Email, Activo "
            + "FROM Cliente "
            + "WHERE Activo = 1 OR (@IdClienteIncluir IS NOT NULL AND IdCliente = @IdClienteIncluir) "
            + "ORDER BY RazonSocial";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdClienteIncluir", (object)idClienteIncluir ?? DBNull.Value)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaClientes();
    }

    public Cliente GetPorId(int idCliente)
    {
        string consulta = "SELECT IdCliente, RazonSocial, Cuit, Email, Activo FROM Cliente WHERE IdCliente = @IdCliente";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdCliente", idCliente)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Cliente cliente = null;

        if (tabla.Rows.Count > 0)
        {
            cliente = tabla.Rows[0].ToCliente();
        }

        return cliente;
    }

    public int Insertar(Cliente cliente)
    {
        string consulta = "INSERT INTO Cliente (RazonSocial, Cuit, Email, Activo) "
            + "VALUES (@RazonSocial, @Cuit, @Email, 1); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        List<SqlParameter> parametros = ArmarParametrosDatos(cliente);

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idCliente = Convert.ToInt32(resultado);
        ActualizarDVH(idCliente);
        return idCliente;
    }

    public void Modificar(Cliente cliente)
    {
        string consulta = "UPDATE Cliente SET RazonSocial = @RazonSocial, Cuit = @Cuit, Email = @Email WHERE IdCliente = @IdCliente";

        List<SqlParameter> parametros = ArmarParametrosDatos(cliente);
        parametros.Add(new SqlParameter("@IdCliente", cliente.IdCliente));

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(cliente.IdCliente);
    }

    public void BajaLogica(int idCliente)
    {
        string consulta = "UPDATE Cliente SET Activo = 0 WHERE IdCliente = @IdCliente";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdCliente", idCliente)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idCliente);
    }

    public bool ExisteCuit(string cuit, int? idClienteExcluir)
    {
        string consulta = "SELECT COUNT(*) FROM Cliente "
            + "WHERE Cuit = @Cuit "
            + "AND (@IdClienteExcluir IS NULL OR IdCliente <> @IdClienteExcluir)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Cuit", cuit),
            new SqlParameter("@IdClienteExcluir", (object)idClienteExcluir ?? DBNull.Value)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado) > 0;
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Cliente", "IdCliente", id);
    }

    private List<SqlParameter> ArmarParametrosDatos(Cliente cliente)
    {
        return new List<SqlParameter>
        {
            new SqlParameter("@RazonSocial", cliente.RazonSocial),
            new SqlParameter("@Cuit", cliente.Cuit),
            new SqlParameter("@Email", cliente.Email)
        };
    }
}

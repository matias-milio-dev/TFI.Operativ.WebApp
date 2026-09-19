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
public class FacturaRepositorio : IFacturaRepositorio, IVerificable
{
    private const string ColumnasConJoin = "F.IdFactura, F.NumeroFactura, F.IdSuscripcion, F.Total, F.Moneda, F.FechaEmision, "
        + "S.IdCliente, C.RazonSocial AS RazonSocialCliente, C.Cuit AS CuitCliente, C.Email AS EmailCliente, "
        + "P.Nombre AS NombrePlan, S.CodigoComprobante AS CodigoComprobantePago";

    private const string DesdeConJoin = "FROM Factura F "
        + "INNER JOIN Suscripcion S ON S.IdSuscripcion = F.IdSuscripcion "
        + "INNER JOIN Cliente C ON C.IdCliente = S.IdCliente "
        + "INNER JOIN [Plan] P ON P.IdPlan = S.IdPlan ";

    private const string Filtros = "WHERE (@IdCliente IS NULL OR S.IdCliente = @IdCliente) "
        + "AND (@Filtro = '' OR F.NumeroFactura LIKE '%' + @Filtro + '%' OR C.RazonSocial LIKE '%' + @Filtro + '%' OR C.Cuit LIKE '%' + @Filtro + '%') ";

    private readonly AccesoDatos accesoDatos;

    public FacturaRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Factura> Listar(string filtro, int? idCliente, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT " + ColumnasConJoin + " " + DesdeConJoin + Filtros
            + "ORDER BY F.FechaEmision DESC, F.IdFactura DESC "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = ArmarParametrosFiltro(filtro, idCliente);
        parametros.Add(new SqlParameter("@Salteo", salteo));
        parametros.Add(new SqlParameter("@TamanioPagina", tamanioPagina));

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaFacturas();
    }

    public int ContarFacturas(string filtro, int? idCliente)
    {
        string consulta = "SELECT COUNT(*) " + DesdeConJoin + Filtros;

        List<SqlParameter> parametros = ArmarParametrosFiltro(filtro, idCliente);

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public Factura GetPorId(int idFactura)
    {
        string consulta = "SELECT " + ColumnasConJoin + " " + DesdeConJoin + "WHERE F.IdFactura = @IdFactura";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdFactura", idFactura)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Factura factura = null;

        if (tabla.Rows.Count > 0)
        {
            factura = tabla.Rows[0].ToFactura();
        }

        return factura;
    }

    public int ContarFacturasDelAnio(int anio)
    {
        string consulta = "SELECT COUNT(*) FROM Factura WHERE YEAR(FechaEmision) = @Anio";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Anio", anio)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public int Insertar(Factura factura)
    {
        string consulta = "INSERT INTO Factura (NumeroFactura, IdSuscripcion, Total, Moneda, FechaEmision) "
            + "VALUES (@NumeroFactura, @IdSuscripcion, @Total, @Moneda, GETDATE()); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NumeroFactura", factura.NumeroFactura),
            new SqlParameter("@IdSuscripcion", factura.IdSuscripcion),
            new SqlParameter("@Total", factura.Total),
            new SqlParameter("@Moneda", factura.Moneda)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idFactura = Convert.ToInt32(resultado);
        ActualizarDVH(idFactura);
        return idFactura;
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Factura", "IdFactura", id);
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
}

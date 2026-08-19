using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Modelos;
using Operativ.DAL.Conexion;
using Operativ.DAL.Contratos;
using Operativ.DAL.Integridad;

namespace Operativ.DAL.Implementaciones;
public class IntegridadRepositorio : IIntegridadRepositorio
{
    private readonly AccesoDatos accesoDatos;
    public IntegridadRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public bool ExisteTablaDigitosVerticiales()
    {
        object resultado = accesoDatos.EjecutarEscalar("SELECT COUNT(*) FROM DigitosVerticales", null);
        return Convert.ToInt32(resultado) > 0;
    }

    public void RecalcularTodo()
    {
        foreach (TablasVerificables tabla in TablasVerificables.ObtenerTodas())
        {
            RecalcularTabla(tabla);
        }
    }

    public List<ResultadoVerificacionTabla> VerificarTodo()
    {
        List<ResultadoVerificacionTabla> resultadosInvalidos = new List<ResultadoVerificacionTabla>();

        foreach (TablasVerificables tabla in TablasVerificables.ObtenerTodas())
        {
            ResultadoVerificacionTabla resultado = VerificarTabla(tabla);

            if (!resultado.Integra)
            {
                resultadosInvalidos.Add(resultado);
            }
        }

        return resultadosInvalidos;
    }

    private void RecalcularTabla(TablasVerificables tabla)
    {
        DataTable filas = accesoDatos.EjecutarReader(string.Format("SELECT * FROM {0}", tabla.Nombre), null);

        foreach (DataRow fila in filas.Rows)
        {
            ActualizarDvhFila(tabla, fila);
        }

        IntegridadHelper.ActualizarDvvTabla(tabla.Nombre);
    }

    private void ActualizarDvhFila(TablasVerificables tabla, DataRow fila)
    {
        List<SqlParameter> parametrosClave = new List<SqlParameter>();
        List<string> condiciones = new List<string>();

        foreach (string columna in tabla.ColumnasClave)
        {
            condiciones.Add(string.Format("{0} = @{0}", columna));
            parametrosClave.Add(new SqlParameter("@" + columna, fila[columna]));
        }

        string condicionWhere = string.Join(" AND ", condiciones);
        long dvh = IntegridadHelper.CalcularDVH(IntegridadHelper.ConstruirCadenaBase(fila));

        IntegridadHelper.EjecutarUpdateDvh(tabla.Nombre, condicionWhere, parametrosClave, dvh);
    }

    private ResultadoVerificacionTabla VerificarTabla(TablasVerificables tabla)
    {
        long dvvAlmacenado = ObtenerDvvAlmacenado(tabla.Nombre);

        DataTable filas = accesoDatos.EjecutarReader(string.Format("SELECT * FROM {0}", tabla.Nombre), null);

        List<long> valoresDvhAlmacenados = new List<long>();
        List<string> clavesFilasInvalidas = new List<string>();

        foreach (DataRow fila in filas.Rows)
        {
            object valorAlmacenado = fila["DVH"];

            if (valorAlmacenado != DBNull.Value)
            {
                valoresDvhAlmacenados.Add(Convert.ToInt64(valorAlmacenado));
            }

            long dvhCalculado = IntegridadHelper.CalcularDVH(IntegridadHelper.ConstruirCadenaBase(fila));

            bool filaValida = valorAlmacenado != DBNull.Value
                && Convert.ToInt64(valorAlmacenado) == dvhCalculado;

            if (!filaValida)
            {
                clavesFilasInvalidas.Add(FormatearClave(tabla, fila));
            }
        }

        long dvvCalculado = IntegridadHelper.CalcularDVV(valoresDvhAlmacenados);

        ResultadoVerificacionTabla resultado = new ResultadoVerificacionTabla
        {
            NombreTabla = tabla.Nombre,
            ValorDvvAlmacenado = dvvAlmacenado,
            ValorDvvCalculado = dvvCalculado,
            Integra = clavesFilasInvalidas.Count == 0 && dvvAlmacenado == dvvCalculado
        };

        if (!resultado.Integra)
        {
            resultado.ClavesFilasInvalidas.AddRange(clavesFilasInvalidas);
        }

        return resultado;
    }

    private string FormatearClave(TablasVerificables tabla, DataRow fila)
    {
        List<string> partes = new List<string>();

        foreach (string columna in tabla.ColumnasClave)
        {
            partes.Add(string.Format("{0}={1}", columna, fila[columna]));
        }

        return string.Join(", ", partes);
    }

    private long ObtenerDvvAlmacenado(string nombreTabla)
    {
        object resultado = accesoDatos.EjecutarEscalar(
            "SELECT ValorDVV FROM DigitosVerticales WHERE NombreTabla = @NombreTabla",
            new List<SqlParameter> { new SqlParameter("@NombreTabla", nombreTabla) });

        if (resultado == null)
        {
            return long.MinValue;
        }

        return Convert.ToInt64(resultado);
    }
}

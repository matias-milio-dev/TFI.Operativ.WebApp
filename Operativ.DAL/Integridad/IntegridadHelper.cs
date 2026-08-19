using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Operativ.DAL.Conexion;

namespace Operativ.DAL.Integridad;
public static class IntegridadHelper
{
    private static readonly AccesoDatos accesoDatos = new AccesoDatos();

    public static string ConstruirCadenaBase(DataRow fila)
    {
        List<string> valores = new List<string>();

        foreach (DataColumn columna in fila.Table.Columns)
        {
            if (string.Equals(columna.ColumnName, "DVH", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            valores.Add(FormatearValor(fila[columna]));
        }
        return string.Join("|", valores);
    }

    public static long CalcularDVH(string cadenaBase)
    {
        long suma = 0;
        for (int posicion = 0; posicion < cadenaBase.Length; posicion++)
        {
            suma = suma + ((long)cadenaBase[posicion] * (posicion + 1));
        }
        return suma;
    }

    public static long CalcularDVV(List<long> valoresDvh)
    {
        long suma = 0;
        foreach (long valorDvh in valoresDvh)
        {
            suma = suma + valorDvh;
        }
        return suma;
    }

    public static void ActualizarIntegridad(string nombreTabla, string columnaId, int id)
    {
        List<SqlParameter> clave = new List<SqlParameter> { new SqlParameter("@" + columnaId, id) };
        string condicionWhere = string.Format("{0} = @{0}", columnaId);

        ActualizarDvhFila(nombreTabla, condicionWhere, clave);
        ActualizarDvvTabla(nombreTabla);
    }

    public static void ActualizarIntegridadClaveCompuesta(string nombreTabla, List<SqlParameter> clavesFila)
    {
        List<string> condiciones = new List<string>();
        foreach (SqlParameter clave in clavesFila)
        {
            string nombreColumna = clave.ParameterName.TrimStart('@');
            condiciones.Add(string.Format("{0} = @{0}", nombreColumna));
        }

        string condicionWhere = string.Join(" AND ", condiciones);

        ActualizarDvhFila(nombreTabla, condicionWhere, clavesFila);
        ActualizarDvvTabla(nombreTabla);
    }

    private static void ActualizarDvhFila(string nombreTabla, string condicionWhere, List<SqlParameter> parametrosClave)
    {
        string consultaSelect = string.Format("SELECT * FROM {0} WHERE {1}", nombreTabla, condicionWhere);
        DataTable filas = accesoDatos.EjecutarReader(consultaSelect, ClonarParametros(parametrosClave));
        DataRow fila = filas.Rows[0];

        long dvh = CalcularDVH(ConstruirCadenaBase(fila));

        EjecutarUpdateDvh(nombreTabla, condicionWhere, parametrosClave, dvh);
    }

    internal static void EjecutarUpdateDvh(string nombreTabla, string condicionWhere, List<SqlParameter> parametrosClave, long dvh)
    {
        string consultaUpdate = string.Format("UPDATE {0} SET DVH = @Dvh WHERE {1}", nombreTabla, condicionWhere);

        List<SqlParameter> parametros = ClonarParametros(parametrosClave);
        parametros.Add(new SqlParameter("@Dvh", dvh));

        accesoDatos.EjecutarConsulta(consultaUpdate, parametros);
    }

    internal static void ActualizarDvvTabla(string nombreTabla)
    {
        DataTable filasDvh = accesoDatos.EjecutarReader(string.Format("SELECT DVH FROM {0}", nombreTabla), null);

        List<long> valoresDvh = new List<long>();

        foreach (DataRow fila in filasDvh.Rows)
        {
            if (fila["DVH"] != DBNull.Value)
            {
                valoresDvh.Add(Convert.ToInt64(fila["DVH"]));
            }
        }

        long dvv = CalcularDVV(valoresDvh);

        int filasActualizadas = accesoDatos.EjecutarConsulta(
            "UPDATE DigitosVerticales SET ValorDVV = @ValorDVV, FechaCalculo = GETDATE() WHERE NombreTabla = @NombreTabla",
            new List<SqlParameter>
            {
                new SqlParameter("@ValorDVV", dvv),
                new SqlParameter("@NombreTabla", nombreTabla)
            });

        if (filasActualizadas == 0)
        {
            accesoDatos.EjecutarConsulta(
                "INSERT INTO DigitosVerticales (NombreTabla, ValorDVV, FechaCalculo) VALUES (@NombreTabla, @ValorDVV, GETDATE())",
                new List<SqlParameter>
                {
                    new SqlParameter("@NombreTabla", nombreTabla),
                    new SqlParameter("@ValorDVV", dvv)
                });
        }
    }

    private static List<SqlParameter> ClonarParametros(List<SqlParameter> parametros)
    {
        List<SqlParameter> clones = new List<SqlParameter>();

        foreach (SqlParameter parametro in parametros)
        {
            clones.Add(new SqlParameter(parametro.ParameterName, parametro.Value));
        }

        return clones;
    }

    private static string FormatearValor(object valor)
    {
        if (valor == null || valor == DBNull.Value)
        {
            return string.Empty;
        }

        if (valor is bool)
        {
            return ((bool)valor) ? "1" : "0";
        }

        if (valor is DateTime)
        {
            return ((DateTime)valor).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        if (valor is IFormattable)
        {
            return ((IFormattable)valor).ToString(null, CultureInfo.InvariantCulture);
        }

        return valor.ToString();
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Operativ.DAL.Conexion;

namespace Operativ.DAL.Integridad;
public static class IntegridadHelper
{
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

        EjecutarActualizacion(nombreTabla, condicionWhere, clave);
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

        EjecutarActualizacion(nombreTabla, condicionWhere, clavesFila);
    }

    private static void EjecutarActualizacion(string nombreTabla, string condicionWhere, List<SqlParameter> parametrosWhere)
    {
        using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
        {
            conexion.Open();

            using (SqlTransaction transaccion = conexion.BeginTransaction())
            {
                try
                {
                    ActualizarDvhFila(conexion, transaccion, nombreTabla, condicionWhere, parametrosWhere);
                    ActualizarDvvTabla(conexion, transaccion, nombreTabla);
                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }
            }
        }
    }

    private static void ActualizarDvhFila(SqlConnection conexion, SqlTransaction transaccion, string nombreTabla, string condicionWhere, List<SqlParameter> parametrosWhere)
    {
        DataRow fila = LeerFila(conexion, transaccion, nombreTabla, condicionWhere, parametrosWhere);
        string cadenaBase = ConstruirCadenaBase(fila);
        long dvh = CalcularDVH(cadenaBase);

        string consultaUpdate = string.Format("UPDATE {0} SET DVH = @Dvh WHERE {1}", nombreTabla, condicionWhere);

        using (SqlCommand comando = new SqlCommand(consultaUpdate, conexion, transaccion))
        {
            comando.Parameters.Add(new SqlParameter("@Dvh", dvh));
            comando.Parameters.AddRange(ClonarParametros(parametrosWhere).ToArray());
            comando.ExecuteNonQuery();
        }
    }

    private static void ActualizarDvvTabla(SqlConnection conexion, SqlTransaction transaccion, string nombreTabla)
    {
        List<long> valoresDvh = new List<long>();

        using (SqlCommand comando = new SqlCommand(string.Format("SELECT DVH FROM {0}", nombreTabla), conexion, transaccion))
        {
            using (SqlDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    if (!lector.IsDBNull(0))
                    {
                        valoresDvh.Add(lector.GetInt64(0));
                    }
                }
            }
        }

        long dvv = CalcularDVV(valoresDvh);
        int filasActualizadas;

        using (SqlCommand comandoUpdate = new SqlCommand(
            "UPDATE DigitosVerticales SET ValorDVV = @ValorDVV, FechaCalculo = GETDATE() WHERE NombreTabla = @NombreTabla",
            conexion, transaccion))
        {
            comandoUpdate.Parameters.Add(new SqlParameter("@ValorDVV", dvv));
            comandoUpdate.Parameters.Add(new SqlParameter("@NombreTabla", nombreTabla));
            filasActualizadas = comandoUpdate.ExecuteNonQuery();
        }

        if (filasActualizadas == 0)
        {
            using (SqlCommand comandoInsert = new SqlCommand(
                "INSERT INTO DigitosVerticales (NombreTabla, ValorDVV, FechaCalculo) VALUES (@NombreTabla, @ValorDVV, GETDATE())",
                conexion, transaccion))
            {
                comandoInsert.Parameters.Add(new SqlParameter("@NombreTabla", nombreTabla));
                comandoInsert.Parameters.Add(new SqlParameter("@ValorDVV", dvv));
                comandoInsert.ExecuteNonQuery();
            }
        }
    }

    private static DataRow LeerFila(SqlConnection conexion, SqlTransaction transaccion, string nombreTabla, string condicionWhere, List<SqlParameter> parametrosWhere)
    {
        DataTable tabla = new DataTable();
        string consulta = string.Format("SELECT * FROM {0} WHERE {1}", nombreTabla, condicionWhere);

        using (SqlCommand comando = new SqlCommand(consulta, conexion, transaccion))
        {
            comando.Parameters.AddRange(ClonarParametros(parametrosWhere).ToArray());

            using (SqlDataReader lector = comando.ExecuteReader())
            {
                tabla.Load(lector);
            }
        }

        return tabla.Rows[0];
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

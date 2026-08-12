using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Operativ.Comun;

namespace Operativ.DAL
{
    internal static class DALHelper
    {
        public static void EjecutarNonQuery(string sql, Action<SqlCommand> configurarParametros)
        {
            using (var conexion = ConexionDB.Instancia.NuevaConexion())
            using (var comando = new SqlCommand(sql, conexion))
            {
                configurarParametros?.Invoke(comando);
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public static DataTable EjecutarConsulta(string sql, Action<SqlCommand> configurarParametros)
        {
            using (var conexion = ConexionDB.Instancia.NuevaConexion())
            using (var adaptador = new SqlDataAdapter(sql, conexion))
            {
                configurarParametros?.Invoke(adaptador.SelectCommand);
                var tabla = new DataTable();
                adaptador.Fill(tabla);
                return tabla;
            }
        }

        public static T EjecutarLector<T>(string sql, Action<SqlCommand> configurarParametros, Func<SqlDataReader, T> mapear) where T : class
        {
            using (var conexion = ConexionDB.Instancia.NuevaConexion())
            using (var comando = new SqlCommand(sql, conexion))
            {
                configurarParametros?.Invoke(comando);
                conexion.Open();
                using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!lector.Read()) return null;
                    return mapear(lector);
                }
            }
        }

        public static T EjecutarEnTransaccion<T>(Func<SqlConnection, SqlTransaction, T> funcion)
        {
            using (var conexion = ConexionDB.Instancia.NuevaConexion())
            {
                conexion.Open();
                using (var transaccion = conexion.BeginTransaction())
                {
                    T resultado = funcion(conexion, transaccion);
                    transaccion.Commit();
                    return resultado;
                }
            }
        }

        public static void EjecutarEnTransaccion(Action<SqlConnection, SqlTransaction> accion)
        {
            EjecutarEnTransaccion<object>((conexion, transaccion) =>
            {
                accion(conexion, transaccion);
                return null;
            });
        }

        public static void ActualizarDigitoVerificadorFila(SqlConnection conexion, SqlTransaction transaccion, string tabla, string columnaId, int id, string valoresConcatenados)
        {
            byte[] dvh = IntegridadHelper.CalcularDigitoVerificador(valoresConcatenados);
            using (var comando = new SqlCommand($"UPDATE dbo.{tabla} SET DVH = @DVH WHERE {columnaId} = @Id", conexion, transaccion))
            {
                comando.Parameters.Add("@DVH", SqlDbType.VarBinary, 32).Value = dvh;
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                comando.ExecuteNonQuery();
            }
        }

        public static void RecalcularDVV(SqlConnection conexion, SqlTransaction transaccion, string tabla, string columnaId)
        {
            var hexConcatenado = new StringBuilder();
            using (var comando = new SqlCommand($"SELECT DVH FROM dbo.{tabla} ORDER BY {columnaId}", conexion, transaccion))
            using (var lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    hexConcatenado.Append(IntegridadHelper.ConvertirAHex(lector["DVH"] as byte[]));
                }
            }

            byte[] dvv = IntegridadHelper.CalcularDigitoVerificador(hexConcatenado.ToString());

            using (var comando = new SqlCommand(
                "UPDATE dbo.DigitoVerificadorTabla SET ValorDVV = @DVV, FechaCalculo = SYSDATETIME() WHERE NombreTabla = @Tabla;" +
                "IF @@ROWCOUNT = 0 INSERT INTO dbo.DigitoVerificadorTabla (NombreTabla, ValorDVV) VALUES (@Tabla, @DVV);", conexion, transaccion))
            {
                comando.Parameters.Add("@DVV", SqlDbType.VarBinary, 32).Value = dvv;
                comando.Parameters.Add("@Tabla", SqlDbType.VarChar, 100).Value = tabla;
                comando.ExecuteNonQuery();
            }
        }

        public static void EjecutarProcedimientoAlmacenado(string nombreProcedimiento, Action<SqlCommand> configurarParametros)
        {
            using (var conexion = ConexionDB.Instancia.NuevaConexion())
            using (var comando = new SqlCommand(nombreProcedimiento, conexion) { CommandType = CommandType.StoredProcedure })
            {
                configurarParametros?.Invoke(comando);
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public static object ValorODbNull(object valor)
        {
            return valor ?? DBNull.Value;
        }
    }
}

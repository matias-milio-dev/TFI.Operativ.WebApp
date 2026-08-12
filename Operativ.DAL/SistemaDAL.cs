using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface ISistemaDAL
    {
        DataTable RepararBaseDatos();
        DataTable VerificarIntegridad();
        bool VerificarIntegridadLogin();
        void RealizarBackup(string rutaDestino);
        void RealizarRestore(string rutaOrigen);
        DataRow ObtenerIndicadoresMonitoreo(int? idCliente);
    }

    public class SistemaDAL : ISistemaDAL
    {
        public DataTable RepararBaseDatos()
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                foreach (var tabla in IntegridadHelper.TablasCriticas.Keys)
                {
                    RecalcularDVHTabla(conexion, transaccion, tabla, IntegridadHelper.TablasCriticas[tabla]);
                    DALHelper.RecalcularDVV(conexion, transaccion, tabla, IntegridadHelper.TablasCriticas[tabla]);
                }

                var resultado = new DataTable();
                using (var adaptador = new SqlDataAdapter(
                    "SELECT NombreTabla, ValorDVV, FechaCalculo FROM dbo.DigitoVerificadorTabla ORDER BY NombreTabla", conexion))
                {
                    adaptador.SelectCommand.Transaction = transaccion;
                    adaptador.Fill(resultado);
                }
                return resultado;
            });
        }

        public DataTable VerificarIntegridad()
        {
            var resultado = new DataTable();
            resultado.Columns.Add("NombreTabla", typeof(string));
            resultado.Columns.Add("DVVAlmacenado", typeof(byte[]));
            resultado.Columns.Add("DVVCalculado", typeof(byte[]));
            resultado.Columns.Add("Integro", typeof(bool));

            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                foreach (var tabla in IntegridadHelper.TablasCriticas.Keys)
                {
                    byte[] dvvCalculado = CalcularDVVDesdeFilas(conexion, transaccion, tabla, IntegridadHelper.TablasCriticas[tabla]);
                    byte[] dvvAlmacenado = ObtenerDVVAlmacenado(conexion, transaccion, tabla);

                    bool integro = dvvAlmacenado != null && EsIgual(dvvAlmacenado, dvvCalculado);
                    resultado.Rows.Add(tabla, dvvAlmacenado, dvvCalculado, integro);
                }
                return 0;
            });

            return resultado;
        }

        public bool VerificarIntegridadLogin()
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                byte[] dvvCalculado = CalcularDVVDesdeFilas(conexion, transaccion, "Usuario", "IdUsuario");
                byte[] dvvAlmacenado = ObtenerDVVAlmacenado(conexion, transaccion, "Usuario");
                return dvvAlmacenado != null && EsIgual(dvvAlmacenado, dvvCalculado);
            });
        }

        public void RealizarBackup(string rutaDestino)
        {
            if (string.IsNullOrEmpty(rutaDestino))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorRutaBackupInvalida);
            }

            DALHelper.EjecutarProcedimientoAlmacenado("dbo.sp_Sistema_RealizarBackup", comando =>
            {
                comando.Parameters.Add("@RutaDestino", SqlDbType.NVarChar, 260).Value = rutaDestino;
            });
        }

        public void RealizarRestore(string rutaOrigen)
        {
            if (string.IsNullOrEmpty(rutaOrigen))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorRutaRestoreInvalida);
            }

            DALHelper.EjecutarProcedimientoAlmacenado("dbo.sp_Sistema_RealizarRestore", comando =>
            {
                comando.Parameters.Add("@RutaOrigen", SqlDbType.NVarChar, 260).Value = rutaOrigen;
            });
        }

        public DataRow ObtenerIndicadoresMonitoreo(int? idCliente)
        {
            var tabla = DALHelper.EjecutarConsulta(@"
                SELECT
                    (SELECT COUNT(*) FROM dbo.Activo WHERE Activo1 = 1 AND (@IdCliente IS NULL OR IdCliente = @IdCliente)) AS ActivosActivos,
                    (SELECT COUNT(*) FROM dbo.Incidente i INNER JOIN dbo.Activo a ON a.IdActivo = i.IdActivo
                        WHERE i.Estado = 'ABIERTO' AND (@IdCliente IS NULL OR a.IdCliente = @IdCliente)) AS IncidentesAbiertos,
                    (SELECT COUNT(*) FROM dbo.Suscripcion s INNER JOIN dbo.EstadoSuscripcion es ON es.IdEstadoSuscripcion = s.IdEstadoSuscripcion
                        WHERE es.Codigo = 'ACTIVA' AND (@IdCliente IS NULL OR s.IdCliente = @IdCliente)) AS SuscripcionesActivas,
                    (SELECT COUNT(*) FROM dbo.Incidente i INNER JOIN dbo.Activo a ON a.IdActivo = i.IdActivo
                        WHERE i.Estado = 'ABIERTO' AND i.Prioridad = 'URGENTE' AND (@IdCliente IS NULL OR a.IdCliente = @IdCliente)) AS AlertasUrgentes",
                comando => comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = DALHelper.ValorODbNull(idCliente));
            return tabla.Rows.Count == 0 ? null : tabla.Rows[0];
        }

        private static void RecalcularDVHTabla(SqlConnection conexion, SqlTransaction transaccion, string tabla, string columnaId)
        {
            var filas = new DataTable();
            using (var adaptador = new SqlDataAdapter($"SELECT * FROM dbo.{tabla} ORDER BY {columnaId}", conexion))
            {
                adaptador.SelectCommand.Transaction = transaccion;
                adaptador.Fill(filas);
            }

            foreach (DataRow fila in filas.Rows)
            {
                var partes = new StringBuilder();
                foreach (DataColumn columna in filas.Columns)
                {
                    if (columna.ColumnName == "DVH") continue;
                    if (partes.Length > 0) partes.Append('|');
                    partes.Append(IntegridadHelper.FormatoValorGenerico(fila[columna]));
                }

                byte[] dvh = IntegridadHelper.CalcularDigitoVerificador(partes.ToString());
                using (var comando = new SqlCommand($"UPDATE dbo.{tabla} SET DVH = @DVH WHERE {columnaId} = @Id", conexion, transaccion))
                {
                    comando.Parameters.Add("@DVH", SqlDbType.VarBinary, 32).Value = dvh;
                    comando.Parameters.Add("@Id", SqlDbType.Int).Value = fila[columnaId];
                    comando.ExecuteNonQuery();
                }
            }
        }

        private static byte[] CalcularDVVDesdeFilas(SqlConnection conexion, SqlTransaction transaccion, string tabla, string columnaId)
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
            return IntegridadHelper.CalcularDigitoVerificador(hexConcatenado.ToString());
        }

        private static byte[] ObtenerDVVAlmacenado(SqlConnection conexion, SqlTransaction transaccion, string tabla)
        {
            using (var comando = new SqlCommand("SELECT ValorDVV FROM dbo.DigitoVerificadorTabla WHERE NombreTabla = @Tabla", conexion, transaccion))
            {
                comando.Parameters.Add("@Tabla", SqlDbType.VarChar, 100).Value = tabla;
                return comando.ExecuteScalar() as byte[];
            }
        }

        private static bool EsIgual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}

using System.Data;
using System.Data.SqlClient;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IActivoDAL
    {
        DataTable ListarPorCliente(int idCliente);
        DataRow ObtenerPorId(int idActivo);
        int Insertar(int idCliente, int? idSuscripcion, string nombre, string tipoActivo, string identificador);
        void Baja(int idActivo);
    }

    public class ActivoDAL : IActivoDAL
    {
        public DataTable ListarPorCliente(int idCliente)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT IdActivo, IdCliente, IdSuscripcion, Nombre, TipoActivo, Identificador, Activo1 AS EstaActivo, FechaAlta
                FROM dbo.Activo WHERE IdCliente = @IdCliente ORDER BY Nombre",
                comando => comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente);
        }

        public DataRow ObtenerPorId(int idActivo)
        {
            var tabla = DALHelper.EjecutarConsulta(@"
                SELECT IdActivo, IdCliente, IdSuscripcion, Nombre, TipoActivo, Identificador, Activo1 AS EstaActivo, FechaAlta
                FROM dbo.Activo WHERE IdActivo = @IdActivo",
                comando => comando.Parameters.Add("@IdActivo", SqlDbType.Int).Value = idActivo);
            return tabla.Rows.Count == 0 ? null : tabla.Rows[0];
        }

        public int Insertar(int idCliente, int? idSuscripcion, string nombre, string tipoActivo, string identificador)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                int idActivoNuevo;
                using (var comando = new SqlCommand(@"
                    INSERT INTO dbo.Activo (IdCliente, IdSuscripcion, Nombre, TipoActivo, Identificador)
                    VALUES (@IdCliente, @IdSuscripcion, @Nombre, @TipoActivo, @Identificador);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;
                    comando.Parameters.Add("@IdSuscripcion", SqlDbType.Int).Value = DALHelper.ValorODbNull(idSuscripcion);
                    comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
                    comando.Parameters.Add("@TipoActivo", SqlDbType.VarChar, 30).Value = tipoActivo;
                    comando.Parameters.Add("@Identificador", SqlDbType.VarChar, 100).Value = DALHelper.ValorODbNull(identificador);
                    idActivoNuevo = (int)comando.ExecuteScalar();
                }

                string valores = string.Join("|", idActivoNuevo.ToString(), idCliente.ToString(), nombre, tipoActivo, IntegridadHelper.FormatoBit(true));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Activo", "IdActivo", idActivoNuevo, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Activo", "IdActivo");

                return idActivoNuevo;
            });
        }

        public void Baja(int idActivo)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                string idCliente;
                string nombre;
                string tipoActivo;
                using (var comando = new SqlCommand("SELECT IdCliente, Nombre, TipoActivo FROM dbo.Activo WHERE IdActivo = @IdActivo", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdActivo", SqlDbType.Int).Value = idActivo;
                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        idCliente = lector["IdCliente"].ToString();
                        nombre = (string)lector["Nombre"];
                        tipoActivo = (string)lector["TipoActivo"];
                    }
                }

                using (var comando = new SqlCommand("UPDATE dbo.Activo SET Activo1 = 0 WHERE IdActivo = @IdActivo", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdActivo", SqlDbType.Int).Value = idActivo;
                    comando.ExecuteNonQuery();
                }

                string valores = string.Join("|", idActivo.ToString(), idCliente, nombre, tipoActivo, IntegridadHelper.FormatoBit(false));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Activo", "IdActivo", idActivo, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Activo", "IdActivo");
            });
        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface ISuscripcionDAL
    {
        DataTable ListarPorCliente(int idCliente);
        DataRow ObtenerPorId(int idSuscripcion);
        int Insertar(int idCliente, int idPaquete, DateTime fechaInicio, DateTime fechaVencimiento,
            decimal precioAcordado, string estrategiaAplicada);
        void CambiarEstado(int idSuscripcion, string codigoEstado);
    }

    public class SuscripcionDAL : ISuscripcionDAL
    {
        public DataTable ListarPorCliente(int idCliente)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT s.IdSuscripcion, s.IdCliente, s.IdPaquete, pa.Nombre AS NombrePaquete, es.Codigo AS CodigoEstado,
                       s.FechaInicio, s.FechaVencimiento, s.PrecioAcordado, s.EstrategiaAplicada
                FROM dbo.Suscripcion s
                INNER JOIN dbo.Paquete pa ON pa.IdPaquete = s.IdPaquete
                INNER JOIN dbo.EstadoSuscripcion es ON es.IdEstadoSuscripcion = s.IdEstadoSuscripcion
                WHERE s.IdCliente = @IdCliente
                ORDER BY s.FechaInicio DESC",
                comando => comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente);
        }

        public DataRow ObtenerPorId(int idSuscripcion)
        {
            var tabla = DALHelper.EjecutarConsulta(@"
                SELECT s.IdSuscripcion, s.IdCliente, s.IdPaquete, es.Codigo AS CodigoEstado,
                       s.FechaInicio, s.FechaVencimiento, s.PrecioAcordado, s.EstrategiaAplicada
                FROM dbo.Suscripcion s
                INNER JOIN dbo.EstadoSuscripcion es ON es.IdEstadoSuscripcion = s.IdEstadoSuscripcion
                WHERE s.IdSuscripcion = @IdSuscripcion",
                comando => comando.Parameters.Add("@IdSuscripcion", SqlDbType.Int).Value = idSuscripcion);
            return tabla.Rows.Count == 0 ? null : tabla.Rows[0];
        }

        public int Insertar(int idCliente, int idPaquete, DateTime fechaInicio, DateTime fechaVencimiento,
            decimal precioAcordado, string estrategiaAplicada)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                int idSuscripcionNueva;
                int idEstadoActiva;
                using (var comando = new SqlCommand(@"
                    DECLARE @IdEstadoActiva INT = (SELECT IdEstadoSuscripcion FROM dbo.EstadoSuscripcion WHERE Codigo = 'ACTIVA');
                    INSERT INTO dbo.Suscripcion (IdCliente, IdPaquete, IdEstadoSuscripcion, FechaInicio, FechaVencimiento, PrecioAcordado, EstrategiaAplicada)
                    VALUES (@IdCliente, @IdPaquete, @IdEstadoActiva, @FechaInicio, @FechaVencimiento, @PrecioAcordado, @EstrategiaAplicada);
                    SELECT CAST(SCOPE_IDENTITY() AS INT), @IdEstadoActiva;", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;
                    comando.Parameters.Add("@IdPaquete", SqlDbType.Int).Value = idPaquete;
                    comando.Parameters.Add("@FechaInicio", SqlDbType.Date).Value = fechaInicio;
                    comando.Parameters.Add("@FechaVencimiento", SqlDbType.Date).Value = fechaVencimiento;
                    comando.Parameters.Add("@PrecioAcordado", SqlDbType.Decimal).Value = precioAcordado;
                    comando.Parameters.Add("@EstrategiaAplicada", SqlDbType.VarChar, 50).Value = DALHelper.ValorODbNull(estrategiaAplicada);

                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        idSuscripcionNueva = (int)lector[0];
                        idEstadoActiva = (int)lector[1];
                    }
                }

                string valores = string.Join("|", idSuscripcionNueva.ToString(), idCliente.ToString(), idPaquete.ToString(),
                    idEstadoActiva.ToString(), IntegridadHelper.FormatoDecimal(precioAcordado));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Suscripcion", "IdSuscripcion", idSuscripcionNueva, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Suscripcion", "IdSuscripcion");

                return idSuscripcionNueva;
            });
        }

        public void CambiarEstado(int idSuscripcion, string codigoEstado)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                int idCliente, idPaquete, idEstado;
                decimal precioAcordado;
                using (var comando = new SqlCommand(@"
                    DECLARE @IdEstado INT = (SELECT IdEstadoSuscripcion FROM dbo.EstadoSuscripcion WHERE Codigo = @CodigoEstado);
                    UPDATE dbo.Suscripcion SET IdEstadoSuscripcion = @IdEstado WHERE IdSuscripcion = @IdSuscripcion;
                    SELECT IdCliente, IdPaquete, @IdEstado, PrecioAcordado FROM dbo.Suscripcion WHERE IdSuscripcion = @IdSuscripcion;", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdSuscripcion", SqlDbType.Int).Value = idSuscripcion;
                    comando.Parameters.Add("@CodigoEstado", SqlDbType.VarChar, 20).Value = codigoEstado;
                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        idCliente = (int)lector[0];
                        idPaquete = (int)lector[1];
                        idEstado = (int)lector[2];
                        precioAcordado = (decimal)lector[3];
                    }
                }

                string valores = string.Join("|", idSuscripcion.ToString(), idCliente.ToString(), idPaquete.ToString(),
                    idEstado.ToString(), IntegridadHelper.FormatoDecimal(precioAcordado));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Suscripcion", "IdSuscripcion", idSuscripcion, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Suscripcion", "IdSuscripcion");
            });
        }
    }
}

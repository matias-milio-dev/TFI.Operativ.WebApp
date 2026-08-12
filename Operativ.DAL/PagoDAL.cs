using System;
using System.Data;
using System.Data.SqlClient;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IPagoDAL
    {
        DataTable ListarPorSuscripcion(int idSuscripcion);
        int Insertar(int idSuscripcion, string codigoMedioPago, decimal monto, string referenciaExterna);
    }

    public class PagoDAL : IPagoDAL
    {
        public DataTable ListarPorSuscripcion(int idSuscripcion)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT p.IdPago, p.IdSuscripcion, mp.Codigo AS CodigoMedioPago, p.Monto, p.FechaPago, p.ReferenciaExterna
                FROM dbo.Pago p
                INNER JOIN dbo.MedioPago mp ON mp.IdMedioPago = p.IdMedioPago
                WHERE p.IdSuscripcion = @IdSuscripcion
                ORDER BY p.FechaPago DESC",
                comando => comando.Parameters.Add("@IdSuscripcion", SqlDbType.Int).Value = idSuscripcion);
        }

        public int Insertar(int idSuscripcion, string codigoMedioPago, decimal monto, string referenciaExterna)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                int idPagoNuevo;
                int idMedioPago;
                DateTime fechaPago;
                using (var comando = new SqlCommand(@"
                    DECLARE @IdMedioPago INT = (SELECT IdMedioPago FROM dbo.MedioPago WHERE Codigo = @CodigoMedioPago);
                    INSERT INTO dbo.Pago (IdSuscripcion, IdMedioPago, Monto, ReferenciaExterna)
                    VALUES (@IdSuscripcion, @IdMedioPago, @Monto, @ReferenciaExterna);
                    SELECT CAST(SCOPE_IDENTITY() AS INT), @IdMedioPago, FechaPago FROM dbo.Pago WHERE IdPago = SCOPE_IDENTITY();", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdSuscripcion", SqlDbType.Int).Value = idSuscripcion;
                    comando.Parameters.Add("@CodigoMedioPago", SqlDbType.VarChar, 20).Value = codigoMedioPago;
                    comando.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
                    comando.Parameters.Add("@ReferenciaExterna", SqlDbType.VarChar, 100).Value = DALHelper.ValorODbNull(referenciaExterna);

                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        idPagoNuevo = (int)lector[0];
                        idMedioPago = (int)lector[1];
                        fechaPago = (DateTime)lector[2];
                    }
                }

                string valores = string.Join("|", idPagoNuevo.ToString(), idSuscripcion.ToString(), idMedioPago.ToString(),
                    IntegridadHelper.FormatoDecimal(monto), IntegridadHelper.FormatoFecha(fechaPago));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Pago", "IdPago", idPagoNuevo, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Pago", "IdPago");

                return idPagoNuevo;
            });
        }
    }
}

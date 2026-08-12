using System.Data;
using System.Data.SqlClient;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IFacturaDAL
    {
        DataTable ListarPorCliente(int idCliente);
        int Insertar(int idPago, string numeroFactura, decimal montoTotal);
    }

    public class FacturaDAL : IFacturaDAL
    {
        public DataTable ListarPorCliente(int idCliente)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT f.IdFactura, f.NumeroFactura, f.FechaEmision, f.MontoTotal, s.IdCliente
                FROM dbo.Factura f
                INNER JOIN dbo.Pago pg ON pg.IdPago = f.IdPago
                INNER JOIN dbo.Suscripcion s ON s.IdSuscripcion = pg.IdSuscripcion
                WHERE s.IdCliente = @IdCliente
                ORDER BY f.FechaEmision DESC",
                comando => comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente);
        }

        public int Insertar(int idPago, string numeroFactura, decimal montoTotal)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                int idFacturaNueva;
                using (var comando = new SqlCommand(@"
                    INSERT INTO dbo.Factura (IdPago, NumeroFactura, MontoTotal) VALUES (@IdPago, @NumeroFactura, @MontoTotal);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdPago", SqlDbType.Int).Value = idPago;
                    comando.Parameters.Add("@NumeroFactura", SqlDbType.VarChar, 20).Value = numeroFactura;
                    comando.Parameters.Add("@MontoTotal", SqlDbType.Decimal).Value = montoTotal;
                    idFacturaNueva = (int)comando.ExecuteScalar();
                }

                string valores = string.Join("|", idFacturaNueva.ToString(), idPago.ToString(), numeroFactura, IntegridadHelper.FormatoDecimal(montoTotal));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Factura", "IdFactura", idFacturaNueva, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Factura", "IdFactura");

                return idFacturaNueva;
            });
        }
    }
}

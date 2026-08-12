using System;
using System.Data;
using System.Data.SqlClient;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IBitacoraDAL
    {
        void Registrar(int? idUsuario, string accion, string entidadAfectada, string idEntidadAfectada,
            string descripcion, string codigoCriticidad, string direccionIp);
        DataTable Listar(DateTime? fechaDesde, DateTime? fechaHasta, int? idUsuario, string accion,
            string codigoCriticidad, int numeroPagina, int tamanioPagina);
    }

    public class BitacoraDAL : IBitacoraDAL
    {
        public void Registrar(int? idUsuario, string accion, string entidadAfectada, string idEntidadAfectada,
            string descripcion, string codigoCriticidad, string direccionIp)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                long idBitacoraNueva;
                DateTime fechaHora;
                int idCriticidad;
                using (var comando = new SqlCommand(@"
                    DECLARE @IdCriticidad INT = (SELECT IdCriticidad FROM dbo.Criticidad WHERE Codigo = @CodigoCriticidad);
                    INSERT INTO dbo.Bitacora (IdUsuario, Accion, EntidadAfectada, IdEntidadAfectada, Descripcion, IdCriticidad, DireccionIP)
                    VALUES (@IdUsuario, @Accion, @EntidadAfectada, @IdEntidadAfectada, @Descripcion, @IdCriticidad, @DireccionIP);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT), FechaHora, @IdCriticidad FROM dbo.Bitacora WHERE IdBitacora = SCOPE_IDENTITY();", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = DALHelper.ValorODbNull(idUsuario);
                    comando.Parameters.Add("@Accion", SqlDbType.VarChar, 50).Value = accion;
                    comando.Parameters.Add("@EntidadAfectada", SqlDbType.VarChar, 50).Value = entidadAfectada;
                    comando.Parameters.Add("@IdEntidadAfectada", SqlDbType.VarChar, 50).Value = DALHelper.ValorODbNull(idEntidadAfectada);
                    comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = DALHelper.ValorODbNull(descripcion);
                    comando.Parameters.Add("@CodigoCriticidad", SqlDbType.VarChar, 20).Value = codigoCriticidad;
                    comando.Parameters.Add("@DireccionIP", SqlDbType.VarChar, 45).Value = DALHelper.ValorODbNull(direccionIp);

                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        idBitacoraNueva = (long)lector[0];
                        fechaHora = (DateTime)lector[1];
                        idCriticidad = (int)lector[2];
                    }
                }

                string valores = string.Join("|", idBitacoraNueva.ToString(), (idUsuario ?? 0).ToString(),
                    IntegridadHelper.FormatoFecha(fechaHora), accion, entidadAfectada, idEntidadAfectada ?? string.Empty, idCriticidad.ToString());
                byte[] dvh = IntegridadHelper.CalcularDigitoVerificador(valores);
                using (var comando = new SqlCommand("UPDATE dbo.Bitacora SET DVH = @DVH WHERE IdBitacora = @Id", conexion, transaccion))
                {
                    comando.Parameters.Add("@DVH", SqlDbType.VarBinary, 32).Value = dvh;
                    comando.Parameters.Add("@Id", SqlDbType.BigInt).Value = idBitacoraNueva;
                    comando.ExecuteNonQuery();
                }
            });
        }

        public DataTable Listar(DateTime? fechaDesde, DateTime? fechaHasta, int? idUsuario, string accion,
            string codigoCriticidad, int numeroPagina, int tamanioPagina)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT b.IdBitacora, b.FechaHora, u.NombreUsuario, b.Accion, b.EntidadAfectada,
                       b.IdEntidadAfectada, b.Descripcion, c.Codigo AS CodigoCriticidad, b.DireccionIP,
                       COUNT(*) OVER() AS TotalRegistros
                FROM dbo.Bitacora b
                LEFT JOIN dbo.Usuario u ON u.IdUsuario = b.IdUsuario
                INNER JOIN dbo.Criticidad c ON c.IdCriticidad = b.IdCriticidad
                WHERE (@FechaDesde IS NULL OR b.FechaHora >= @FechaDesde)
                  AND (@FechaHasta IS NULL OR b.FechaHora <= @FechaHasta)
                  AND (@IdUsuario IS NULL OR b.IdUsuario = @IdUsuario)
                  AND (@Accion IS NULL OR b.Accion = @Accion)
                  AND (@CodigoCriticidad IS NULL OR c.Codigo = @CodigoCriticidad)
                ORDER BY b.FechaHora DESC
                OFFSET (@NumeroPagina - 1) * @TamanioPagina ROWS FETCH NEXT @TamanioPagina ROWS ONLY", comando =>
            {
                comando.Parameters.Add("@FechaDesde", SqlDbType.DateTime2).Value = DALHelper.ValorODbNull(fechaDesde);
                comando.Parameters.Add("@FechaHasta", SqlDbType.DateTime2).Value = DALHelper.ValorODbNull(fechaHasta);
                comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = DALHelper.ValorODbNull(idUsuario);
                comando.Parameters.Add("@Accion", SqlDbType.VarChar, 50).Value = DALHelper.ValorODbNull(accion);
                comando.Parameters.Add("@CodigoCriticidad", SqlDbType.VarChar, 20).Value = DALHelper.ValorODbNull(codigoCriticidad);
                comando.Parameters.Add("@NumeroPagina", SqlDbType.Int).Value = numeroPagina;
                comando.Parameters.Add("@TamanioPagina", SqlDbType.Int).Value = tamanioPagina;
            });
        }
    }
}

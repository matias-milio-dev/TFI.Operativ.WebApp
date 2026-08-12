using System.Data;
using System.Data.SqlClient;
using Operativ.BE;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IPaqueteDAL
    {
        DataTable Listar(bool soloActivos);
        Paquete ObtenerPorId(int idPaquete);
        int Insertar(Paquete paquete);
        void Modificar(Paquete paquete);
        void Baja(int idPaquete);
    }

    public class PaqueteDAL : IPaqueteDAL
    {
        public DataTable Listar(bool soloActivos)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT IdPaquete, Nombre, Descripcion, PrecioBase, CantidadActivosIncluidos, Activo
                FROM dbo.Paquete
                WHERE (@SoloActivos = 0 OR Activo = 1)
                ORDER BY PrecioBase",
                comando => comando.Parameters.Add("@SoloActivos", SqlDbType.Bit).Value = soloActivos);
        }

        public Paquete ObtenerPorId(int idPaquete)
        {
            var tabla = DALHelper.EjecutarConsulta(
                "SELECT IdPaquete, Nombre, Descripcion, PrecioBase, CantidadActivosIncluidos, Activo FROM dbo.Paquete WHERE IdPaquete = @IdPaquete",
                comando => comando.Parameters.Add("@IdPaquete", SqlDbType.Int).Value = idPaquete);
            if (tabla.Rows.Count == 0) return null;
            var fila = tabla.Rows[0];
            return new Paquete
            {
                IdPaquete = (int)fila["IdPaquete"],
                Nombre = (string)fila["Nombre"],
                Descripcion = fila["Descripcion"] as string,
                PrecioBase = (decimal)fila["PrecioBase"],
                CantidadActivosIncluidos = (int)fila["CantidadActivosIncluidos"],
                Activo = (bool)fila["Activo"]
            };
        }

        public int Insertar(Paquete paquete)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                int idPaqueteNuevo;
                using (var comando = new SqlCommand(@"
                    INSERT INTO dbo.Paquete (Nombre, Descripcion, PrecioBase, CantidadActivosIncluidos)
                    VALUES (@Nombre, @Descripcion, @PrecioBase, @CantidadActivosIncluidos);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion, transaccion))
                {
                    comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = paquete.Nombre;
                    comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 400).Value = DALHelper.ValorODbNull(paquete.Descripcion);
                    comando.Parameters.Add("@PrecioBase", SqlDbType.Decimal).Value = paquete.PrecioBase;
                    comando.Parameters.Add("@CantidadActivosIncluidos", SqlDbType.Int).Value = paquete.CantidadActivosIncluidos;
                    idPaqueteNuevo = (int)comando.ExecuteScalar();
                }

                string valores = string.Join("|", idPaqueteNuevo.ToString(), paquete.Nombre, IntegridadHelper.FormatoDecimal(paquete.PrecioBase), IntegridadHelper.FormatoBit(true));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Paquete", "IdPaquete", idPaqueteNuevo, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Paquete", "IdPaquete");

                return idPaqueteNuevo;
            });
        }

        public void Modificar(Paquete paquete)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comando = new SqlCommand(@"
                    UPDATE dbo.Paquete SET Nombre = @Nombre, Descripcion = @Descripcion, PrecioBase = @PrecioBase,
                           CantidadActivosIncluidos = @CantidadActivosIncluidos
                    WHERE IdPaquete = @IdPaquete", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdPaquete", SqlDbType.Int).Value = paquete.IdPaquete;
                    comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = paquete.Nombre;
                    comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 400).Value = DALHelper.ValorODbNull(paquete.Descripcion);
                    comando.Parameters.Add("@PrecioBase", SqlDbType.Decimal).Value = paquete.PrecioBase;
                    comando.Parameters.Add("@CantidadActivosIncluidos", SqlDbType.Int).Value = paquete.CantidadActivosIncluidos;
                    comando.ExecuteNonQuery();
                }

                bool activo;
                using (var comando = new SqlCommand("SELECT Activo FROM dbo.Paquete WHERE IdPaquete = @IdPaquete", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdPaquete", SqlDbType.Int).Value = paquete.IdPaquete;
                    activo = (bool)comando.ExecuteScalar();
                }

                string valores = string.Join("|", paquete.IdPaquete.ToString(), paquete.Nombre, IntegridadHelper.FormatoDecimal(paquete.PrecioBase), IntegridadHelper.FormatoBit(activo));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Paquete", "IdPaquete", paquete.IdPaquete, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Paquete", "IdPaquete");
            });
        }

        public void Baja(int idPaquete)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                string nombre;
                decimal precioBase;
                using (var comando = new SqlCommand("SELECT Nombre, PrecioBase FROM dbo.Paquete WHERE IdPaquete = @IdPaquete", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdPaquete", SqlDbType.Int).Value = idPaquete;
                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        nombre = (string)lector["Nombre"];
                        precioBase = (decimal)lector["PrecioBase"];
                    }
                }

                using (var comando = new SqlCommand("UPDATE dbo.Paquete SET Activo = 0 WHERE IdPaquete = @IdPaquete", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdPaquete", SqlDbType.Int).Value = idPaquete;
                    comando.ExecuteNonQuery();
                }

                string valores = string.Join("|", idPaquete.ToString(), nombre, IntegridadHelper.FormatoDecimal(precioBase), IntegridadHelper.FormatoBit(false));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Paquete", "IdPaquete", idPaquete, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Paquete", "IdPaquete");
            });
        }
    }
}

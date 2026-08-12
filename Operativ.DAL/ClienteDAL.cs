using System;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IClienteDAL
    {
        DataTable Listar(string filtro, int numeroPagina, int tamanioPagina);
        Cliente ObtenerPorId(int idCliente);
        Cliente ObtenerPorCuit(string cuit);
        int Insertar(Cliente cliente);
        void Modificar(Cliente cliente);
        void Baja(int idCliente);
    }

    public class ClienteDAL : IClienteDAL
    {
        public DataTable Listar(string filtro, int numeroPagina, int tamanioPagina)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT IdCliente, Cuit, RazonSocial, CorreoElectronico, Telefono, Activo, FechaAlta,
                       COUNT(*) OVER() AS TotalRegistros
                FROM dbo.Cliente
                WHERE (@Filtro IS NULL OR RazonSocial LIKE '%' + @Filtro + '%' OR Cuit LIKE '%' + @Filtro + '%')
                ORDER BY RazonSocial
                OFFSET (@NumeroPagina - 1) * @TamanioPagina ROWS FETCH NEXT @TamanioPagina ROWS ONLY", comando =>
            {
                comando.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = DALHelper.ValorODbNull(filtro);
                comando.Parameters.Add("@NumeroPagina", SqlDbType.Int).Value = numeroPagina;
                comando.Parameters.Add("@TamanioPagina", SqlDbType.Int).Value = tamanioPagina;
            });
        }

        public Cliente ObtenerPorId(int idCliente)
        {
            var tabla = DALHelper.EjecutarConsulta(@"
                SELECT IdCliente, Cuit, RazonSocial, CorreoElectronico, Telefono, Direccion, IdUsuario, Activo, FechaAlta
                FROM dbo.Cliente WHERE IdCliente = @IdCliente",
                comando => comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente);
            return tabla.Rows.Count == 0 ? null : MapearCliente(tabla.Rows[0]);
        }

        public Cliente ObtenerPorCuit(string cuit)
        {
            var tabla = DALHelper.EjecutarConsulta(@"
                SELECT IdCliente, Cuit, RazonSocial, CorreoElectronico, Telefono, Direccion, IdUsuario, Activo, FechaAlta
                FROM dbo.Cliente WHERE Cuit = @Cuit",
                comando => comando.Parameters.Add("@Cuit", SqlDbType.VarChar, 13).Value = cuit);
            return tabla.Rows.Count == 0 ? null : MapearCliente(tabla.Rows[0]);
        }

        public int Insertar(Cliente cliente)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comandoDuplicado = new SqlCommand("SELECT COUNT(*) FROM dbo.Cliente WHERE Cuit = @Cuit", conexion, transaccion))
                {
                    comandoDuplicado.Parameters.Add("@Cuit", SqlDbType.VarChar, 13).Value = cliente.Cuit;
                    if ((int)comandoDuplicado.ExecuteScalar() > 0)
                    {
                        throw new ExcepcionNegocio(CodigosError.ErrorCuitClienteDuplicado);
                    }
                }

                int idClienteNuevo;
                using (var comando = new SqlCommand(@"
                    INSERT INTO dbo.Cliente (Cuit, RazonSocial, CorreoElectronico, Telefono, Direccion, IdUsuario)
                    VALUES (@Cuit, @RazonSocial, @CorreoElectronico, @Telefono, @Direccion, @IdUsuario);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion, transaccion))
                {
                    comando.Parameters.Add("@Cuit", SqlDbType.VarChar, 13).Value = cliente.Cuit;
                    comando.Parameters.Add("@RazonSocial", SqlDbType.NVarChar, 150).Value = cliente.RazonSocial;
                    comando.Parameters.Add("@CorreoElectronico", SqlDbType.VarChar, 150).Value = cliente.CorreoElectronico;
                    comando.Parameters.Add("@Telefono", SqlDbType.VarChar, 30).Value = DALHelper.ValorODbNull(cliente.Telefono);
                    comando.Parameters.Add("@Direccion", SqlDbType.NVarChar, 200).Value = DALHelper.ValorODbNull(cliente.Direccion);
                    comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = DALHelper.ValorODbNull(cliente.IdUsuario);
                    idClienteNuevo = (int)comando.ExecuteScalar();
                }

                string valores = string.Join("|", idClienteNuevo.ToString(), cliente.Cuit, cliente.RazonSocial, cliente.CorreoElectronico, IntegridadHelper.FormatoBit(true));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Cliente", "IdCliente", idClienteNuevo, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Cliente", "IdCliente");

                return idClienteNuevo;
            });
        }

        public void Modificar(Cliente cliente)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comando = new SqlCommand(@"
                    UPDATE dbo.Cliente
                    SET RazonSocial = @RazonSocial, CorreoElectronico = @CorreoElectronico, Telefono = @Telefono, Direccion = @Direccion
                    WHERE IdCliente = @IdCliente", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = cliente.IdCliente;
                    comando.Parameters.Add("@RazonSocial", SqlDbType.NVarChar, 150).Value = cliente.RazonSocial;
                    comando.Parameters.Add("@CorreoElectronico", SqlDbType.VarChar, 150).Value = cliente.CorreoElectronico;
                    comando.Parameters.Add("@Telefono", SqlDbType.VarChar, 30).Value = DALHelper.ValorODbNull(cliente.Telefono);
                    comando.Parameters.Add("@Direccion", SqlDbType.NVarChar, 200).Value = DALHelper.ValorODbNull(cliente.Direccion);
                    comando.ExecuteNonQuery();
                }

                string cuit;
                bool activo;
                using (var comando = new SqlCommand("SELECT Cuit, Activo FROM dbo.Cliente WHERE IdCliente = @IdCliente", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = cliente.IdCliente;
                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        cuit = (string)lector["Cuit"];
                        activo = (bool)lector["Activo"];
                    }
                }

                string valores = string.Join("|", cliente.IdCliente.ToString(), cuit, cliente.RazonSocial, cliente.CorreoElectronico, IntegridadHelper.FormatoBit(activo));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Cliente", "IdCliente", cliente.IdCliente, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Cliente", "IdCliente");
            });
        }

        public void Baja(int idCliente)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comando = new SqlCommand("UPDATE dbo.Cliente SET Activo = 0 WHERE IdCliente = @IdCliente", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;
                    comando.ExecuteNonQuery();
                }

                string cuit, razonSocial, correo;
                using (var comando = new SqlCommand("SELECT Cuit, RazonSocial, CorreoElectronico FROM dbo.Cliente WHERE IdCliente = @IdCliente", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;
                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        cuit = (string)lector["Cuit"];
                        razonSocial = (string)lector["RazonSocial"];
                        correo = (string)lector["CorreoElectronico"];
                    }
                }

                string valores = string.Join("|", idCliente.ToString(), cuit, razonSocial, correo, IntegridadHelper.FormatoBit(false));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Cliente", "IdCliente", idCliente, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Cliente", "IdCliente");
            });
        }

        private static Cliente MapearCliente(DataRow fila)
        {
            return new Cliente
            {
                IdCliente = (int)fila["IdCliente"],
                Cuit = (string)fila["Cuit"],
                RazonSocial = (string)fila["RazonSocial"],
                CorreoElectronico = (string)fila["CorreoElectronico"],
                Telefono = fila["Telefono"] as string,
                Direccion = fila.Table.Columns.Contains("Direccion") ? fila["Direccion"] as string : null,
                IdUsuario = fila.Table.Columns.Contains("IdUsuario") && fila["IdUsuario"] != DBNull.Value ? (int?)fila["IdUsuario"] : null,
                Activo = (bool)fila["Activo"],
                FechaAlta = (DateTime)fila["FechaAlta"]
            };
        }
    }
}

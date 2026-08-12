using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Operativ.BE;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IFamiliaDAL
    {
        List<Familia> Listar();
        Familia ObtenerPorId(int idFamilia);
        int Insertar(Familia familia);
        void Modificar(Familia familia);
        void Baja(int idFamilia);
    }

    public class FamiliaDAL : IFamiliaDAL
    {
        public List<Familia> Listar()
        {
            var familias = new List<Familia>();
            var tabla = DALHelper.EjecutarConsulta("SELECT IdFamilia, Nombre, Descripcion, Activo FROM dbo.Familia ORDER BY Nombre", null);
            foreach (DataRow fila in tabla.Rows)
            {
                familias.Add(new Familia
                {
                    IdFamilia = (int)fila["IdFamilia"],
                    Nombre = (string)fila["Nombre"],
                    Descripcion = fila["Descripcion"] as string,
                    Activo = (bool)fila["Activo"]
                });
            }
            return familias;
        }

        public Familia ObtenerPorId(int idFamilia)
        {
            var tabla = DALHelper.EjecutarConsulta("SELECT IdFamilia, Nombre, Descripcion, Activo FROM dbo.Familia WHERE IdFamilia = @IdFamilia",
                comando => comando.Parameters.Add("@IdFamilia", SqlDbType.Int).Value = idFamilia);
            if (tabla.Rows.Count == 0) return null;
            var fila = tabla.Rows[0];
            return new Familia
            {
                IdFamilia = (int)fila["IdFamilia"],
                Nombre = (string)fila["Nombre"],
                Descripcion = fila["Descripcion"] as string,
                Activo = (bool)fila["Activo"]
            };
        }

        public int Insertar(Familia familia)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                int idFamiliaNueva;
                using (var comando = new SqlCommand(@"
                    INSERT INTO dbo.Familia (Nombre, Descripcion) VALUES (@Nombre, @Descripcion);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion, transaccion))
                {
                    comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = familia.Nombre;
                    comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 300).Value = DALHelper.ValorODbNull(familia.Descripcion);
                    idFamiliaNueva = (int)comando.ExecuteScalar();
                }

                string valores = string.Join("|", idFamiliaNueva.ToString(), familia.Nombre, IntegridadHelper.FormatoBit(true));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Familia", "IdFamilia", idFamiliaNueva, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Familia", "IdFamilia");

                return idFamiliaNueva;
            });
        }

        public void Modificar(Familia familia)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comando = new SqlCommand("UPDATE dbo.Familia SET Nombre = @Nombre, Descripcion = @Descripcion WHERE IdFamilia = @IdFamilia", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdFamilia", SqlDbType.Int).Value = familia.IdFamilia;
                    comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = familia.Nombre;
                    comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 300).Value = DALHelper.ValorODbNull(familia.Descripcion);
                    comando.ExecuteNonQuery();
                }

                bool activo;
                using (var comando = new SqlCommand("SELECT Activo FROM dbo.Familia WHERE IdFamilia = @IdFamilia", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdFamilia", SqlDbType.Int).Value = familia.IdFamilia;
                    activo = (bool)comando.ExecuteScalar();
                }

                string valores = string.Join("|", familia.IdFamilia.ToString(), familia.Nombre, IntegridadHelper.FormatoBit(activo));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Familia", "IdFamilia", familia.IdFamilia, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Familia", "IdFamilia");
            });
        }

        public void Baja(int idFamilia)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                string nombre;
                using (var comando = new SqlCommand("SELECT Nombre FROM dbo.Familia WHERE IdFamilia = @IdFamilia", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdFamilia", SqlDbType.Int).Value = idFamilia;
                    nombre = (string)comando.ExecuteScalar();
                }

                using (var comando = new SqlCommand("UPDATE dbo.Familia SET Activo = 0 WHERE IdFamilia = @IdFamilia", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdFamilia", SqlDbType.Int).Value = idFamilia;
                    comando.ExecuteNonQuery();
                }

                string valores = string.Join("|", idFamilia.ToString(), nombre, IntegridadHelper.FormatoBit(false));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Familia", "IdFamilia", idFamilia, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Familia", "IdFamilia");
            });
        }
    }
}

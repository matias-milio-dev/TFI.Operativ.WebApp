using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IPermisosDAL
    {
        void AsignarFamiliaAPatente(int idFamilia, int idPatente);
        void RemoverFamiliaDePatente(int idFamilia, int idPatente);
        void AsignarUsuarioAFamilia(int idUsuario, int idFamilia);
        void RemoverUsuarioDeFamilia(int idUsuario, int idFamilia);
        void AsignarPatenteDirectaAUsuario(int idUsuario, int idPatente);
        void RemoverPatenteDirectaDeUsuario(int idUsuario, int idPatente);
        DataTable ListarPatentesDeFamilia(int idFamilia);
        List<Patente> ObtenerPatentesDeFamilia(int idFamilia);
    }

    public class PermisosDAL : IPermisosDAL
    {
        public void AsignarFamiliaAPatente(int idFamilia, int idPatente)
        {
            AsignarRelacion("FamiliaPatente", "IdFamiliaPatente", "IdFamilia", "IdPatente", idFamilia, idPatente);
        }

        public void RemoverFamiliaDePatente(int idFamilia, int idPatente)
        {
            RemoverRelacion("FamiliaPatente", "IdFamiliaPatente", "IdFamilia", "IdPatente", idFamilia, idPatente);
        }

        public void AsignarUsuarioAFamilia(int idUsuario, int idFamilia)
        {
            AsignarRelacion("UsuarioFamilia", "IdUsuarioFamilia", "IdUsuario", "IdFamilia", idUsuario, idFamilia);
        }

        public void RemoverUsuarioDeFamilia(int idUsuario, int idFamilia)
        {
            RemoverRelacion("UsuarioFamilia", "IdUsuarioFamilia", "IdUsuario", "IdFamilia", idUsuario, idFamilia);
        }

        public void AsignarPatenteDirectaAUsuario(int idUsuario, int idPatente)
        {
            AsignarRelacion("UsuarioPatente", "IdUsuarioPatente", "IdUsuario", "IdPatente", idUsuario, idPatente);
        }

        public void RemoverPatenteDirectaDeUsuario(int idUsuario, int idPatente)
        {
            RemoverRelacion("UsuarioPatente", "IdUsuarioPatente", "IdUsuario", "IdPatente", idUsuario, idPatente);
        }

        public DataTable ListarPatentesDeFamilia(int idFamilia)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT p.IdPatente, p.Codigo, p.Nombre, p.Modulo
                FROM dbo.FamiliaPatente fp
                INNER JOIN dbo.Patente p ON p.IdPatente = fp.IdPatente
                WHERE fp.IdFamilia = @IdFamilia
                ORDER BY p.Modulo, p.Nombre",
                comando => comando.Parameters.Add("@IdFamilia", SqlDbType.Int).Value = idFamilia);
        }

        public List<Patente> ObtenerPatentesDeFamilia(int idFamilia)
        {
            var patentes = new List<Patente>();
            foreach (DataRow fila in ListarPatentesDeFamilia(idFamilia).Rows)
            {
                patentes.Add(new Patente
                {
                    IdPatente = (int)fila["IdPatente"],
                    Codigo = (string)fila["Codigo"],
                    Nombre = (string)fila["Nombre"],
                    Modulo = (string)fila["Modulo"]
                });
            }
            return patentes;
        }

        private static void AsignarRelacion(string tabla, string columnaId, string columnaA, string columnaB, int valorA, int valorB)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comandoExiste = new SqlCommand($"SELECT COUNT(*) FROM dbo.{tabla} WHERE {columnaA} = @ValorA AND {columnaB} = @ValorB", conexion, transaccion))
                {
                    comandoExiste.Parameters.Add("@ValorA", SqlDbType.Int).Value = valorA;
                    comandoExiste.Parameters.Add("@ValorB", SqlDbType.Int).Value = valorB;
                    if ((int)comandoExiste.ExecuteScalar() > 0) return;
                }

                int idNuevo;
                using (var comando = new SqlCommand(
                    $"INSERT INTO dbo.{tabla} ({columnaA}, {columnaB}) VALUES (@ValorA, @ValorB); SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion, transaccion))
                {
                    comando.Parameters.Add("@ValorA", SqlDbType.Int).Value = valorA;
                    comando.Parameters.Add("@ValorB", SqlDbType.Int).Value = valorB;
                    idNuevo = (int)comando.ExecuteScalar();
                }

                string valores = string.Join("|", idNuevo.ToString(), valorA.ToString(), valorB.ToString());
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, tabla, columnaId, idNuevo, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, tabla, columnaId);
            });
        }

        private static void RemoverRelacion(string tabla, string columnaId, string columnaA, string columnaB, int valorA, int valorB)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comando = new SqlCommand($"DELETE FROM dbo.{tabla} WHERE {columnaA} = @ValorA AND {columnaB} = @ValorB", conexion, transaccion))
                {
                    comando.Parameters.Add("@ValorA", SqlDbType.Int).Value = valorA;
                    comando.Parameters.Add("@ValorB", SqlDbType.Int).Value = valorB;
                    comando.ExecuteNonQuery();
                }

                DALHelper.RecalcularDVV(conexion, transaccion, tabla, columnaId);
            });
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;
using Operativ.DAL.Conexion;

namespace Operativ.DAL.Implementaciones
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly AccesoDatos accesoDatos;

        public UsuarioRepositorio()
        {
            accesoDatos = new AccesoDatos();
        }

        public Usuario GetPorNombreUsuario(string nombreUsuario)
        {
            string consulta = "SELECT IdUsuario, NombreUsuario, Contrasena, Salt, Email, NombreCompleto, Bloqueado, IntentosFallidos, Activo "
                + "FROM Usuario WHERE NombreUsuario = @NombreUsuario";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@NombreUsuario", nombreUsuario)
            };

            DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

            Usuario usuario = null;

            if (tabla.Rows.Count > 0)
            {
                usuario = tabla.Rows[0].ToUsuario();
            }

            return usuario;
        }

        public void ActualizarIntentosFallidos(int idUsuario, int intentosFallidos, bool bloqueado)
        {
            string consulta = "UPDATE Usuario SET IntentosFallidos = @IntentosFallidos, Bloqueado = @Bloqueado WHERE IdUsuario = @IdUsuario";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@IntentosFallidos", intentosFallidos),
                new SqlParameter("@Bloqueado", bloqueado),
                new SqlParameter("@IdUsuario", idUsuario)
            };

            accesoDatos.EjecutarConsulta(consulta, parametros);
        }

        public void ActualizarContrasena(int idUsuario, string contrasena, string salt)
        {
            string consulta = "UPDATE Usuario SET Contrasena = @Contrasena, Salt = @Salt WHERE IdUsuario = @IdUsuario";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@Contrasena", contrasena),
                new SqlParameter("@Salt", salt),
                new SqlParameter("@IdUsuario", idUsuario)
            };

            accesoDatos.EjecutarConsulta(consulta, parametros);
        }

        public void ResetearIntentosFallidos(int idUsuario)
        {
            string consulta = "UPDATE Usuario SET IntentosFallidos = 0 WHERE IdUsuario = @IdUsuario";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", idUsuario)
            };

            accesoDatos.EjecutarConsulta(consulta, parametros);
        }
    }
}

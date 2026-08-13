using System.Data;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Convertidores
{
    public static class UsuarioConvertidor
    {
        public static Usuario ToUsuario(this DataRow fila)
        {
            Usuario usuario = new Usuario
            {
                IdUsuario = (int)fila["IdUsuario"],
                NombreUsuario = fila["NombreUsuario"].ToString(),
                Contrasena = fila["Contrasena"].ToString(),
                Salt = fila["Salt"].ToString(),
                Email = fila["Email"].ToString(),
                NombreCompleto = fila["NombreCompleto"].ToString(),
                Bloqueado = (bool)fila["Bloqueado"],
                IntentosFallidos = (int)fila["IntentosFallidos"],
                Activo = (bool)fila["Activo"]
            };
            return usuario;
        }
    }
}

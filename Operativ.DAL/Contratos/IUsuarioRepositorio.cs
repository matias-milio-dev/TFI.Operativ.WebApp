using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos
{
    public interface IUsuarioRepositorio
    {
        Usuario GetPorNombreUsuario(string nombreUsuario);

        void ActualizarIntentosFallidos(int idUsuario, int intentosFallidos, bool bloqueado);

        void ActualizarContrasena(int idUsuario, string contrasena, string salt);

        void ResetearIntentosFallidos(int idUsuario);
    }
}

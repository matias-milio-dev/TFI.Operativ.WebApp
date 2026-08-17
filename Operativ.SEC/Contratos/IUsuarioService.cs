using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos
{
    public interface IUsuarioService
    {
        Usuario ValidarCredenciales(string nombreUsuario, string contrasena);

        void RecuperarContrasena(string nombreUsuario);
    }
}

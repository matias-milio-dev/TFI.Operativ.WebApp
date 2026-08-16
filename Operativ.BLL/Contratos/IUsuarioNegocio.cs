using Operativ.BE.Entidades;

namespace Operativ.BLL.Contratos
{
    public interface IUsuarioNegocio
    {
        Usuario ValidarCredenciales(string nombreUsuario, string contrasena);

        void RecuperarContrasena(string nombreUsuario);

        void RegistrarCierreSesion(int idUsuario);
    }
}

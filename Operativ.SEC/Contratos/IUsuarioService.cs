using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos
{
    public interface IUsuarioService
    {
        Usuario ValidarCredenciales(string nombreUsuario, string contrasena);

        void RecuperarContrasena(string nombreUsuario);

        int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int idFamilia, int idUsuarioEjecutor);

        void ModificarUsuario(Usuario usuario, int idUsuarioEjecutor);

        void BajaUsuario(int idUsuario, int idUsuarioEjecutor);

        Usuario ObtenerUsuarioPorId(int idUsuario);

        List<Usuario> ListarUsuarios(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina);

        int ContarUsuarios(string filtro, int? idFamilia);
    }
}

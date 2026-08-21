using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IUsuarioService
{
    void RecuperarContrasena(string nombreUsuario);

    void CambiarClave(int idUsuario, string claveActual, string claveNueva);

    int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int idFamilia);

    void ModificarUsuario(Usuario usuario);

    void BajaUsuario(int idUsuario);

    void DesbloquearUsuario(int idUsuario);

    Usuario ObtenerUsuarioPorId(int idUsuario);

    List<Usuario> ListarUsuarios(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina);

    int ContarUsuarios(string filtro, int? idFamilia);
}

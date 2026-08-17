using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos
{
    public interface IUsuarioRepositorio
    {
        Usuario GetPorNombreUsuario(string nombreUsuario);

        Usuario GetPorId(int idUsuario);

        void ActualizarIntentosFallidos(int idUsuario, int intentosFallidos, bool bloqueado);

        void ActualizarContrasena(int idUsuario, string contrasena, string salt);

        void ResetearIntentosFallidos(int idUsuario);

        int Insertar(Usuario usuario);

        void Modificar(Usuario usuario);

        void BajaLogica(int idUsuario);

        void AsignarFamilia(int idUsuario, int idFamilia);

        List<Usuario> Listar(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina);

        int ContarUsuarios(string filtro, int? idFamilia);

        bool ExisteNombreUsuario(string nombreUsuario, int? idUsuarioExcluir);

        bool ExisteEmail(string correoElectronico, int? idUsuarioExcluir);
    }
}

using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.SEC.Helpers;

namespace Operativ.SEC.Implementaciones;
public partial class UsuarioService
{
    public int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int idFamilia)
    {
        ValidarUnicidad(nombreUsuario, correoElectronico, null);

        string contrasenaTemporal = ClaveHelper.GenerarContrasenaTemporal();
        string salt = HashHelper.GenerarSalt();
        string hash = HashHelper.GenerarHash(contrasenaTemporal, salt);

        EmailHelper.EnviarBienvenida(correoElectronico, nombreUsuario, contrasenaTemporal);

        Usuario usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            Email = correoElectronico,
            Contrasena = hash,
            Salt = salt
        };

        int idUsuario = usuarioRepositorio.Insertar(usuario);
        usuarioRepositorio.AsignarFamilia(idUsuario, idFamilia);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.AltaUsuario);

        return idUsuario;
    }

    public void ModificarUsuario(Usuario usuario)
    {
        ValidarUnicidad(usuario.NombreUsuario, usuario.Email, usuario.IdUsuario);

        usuarioRepositorio.Modificar(usuario);

        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.ModificacionUsuario);
    }

    public void BajaUsuario(int idUsuario)
    {
        usuarioRepositorio.BajaLogica(idUsuario);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.BajaUsuario);
    }

    public Usuario ObtenerUsuarioPorId(int idUsuario)
    {
        Usuario usuario = usuarioRepositorio.GetPorId(idUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);

        usuario.Familias = familiaRepositorio.GetFamiliasDeUsuario(idUsuario);

        return usuario;
    }

    public List<Usuario> ListarUsuarios(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina)
    {
        return usuarioRepositorio.Listar(filtro, idFamilia, numeroPagina, tamanioPagina);
    }

    public int ContarUsuarios(string filtro, int? idFamilia)
    {
        return usuarioRepositorio.ContarUsuarios(filtro, idFamilia);
    }

    private void ValidarUnicidad(string nombreUsuario, string correoElectronico, int? idUsuarioExcluir)
    {
        if (usuarioRepositorio.ExisteNombreUsuario(nombreUsuario, idUsuarioExcluir))
        {
            throw new OperativException(TipoError.ErrorUsuarioYaExiste);
        }

        if (usuarioRepositorio.ExisteEmail(correoElectronico, idUsuarioExcluir))
        {
            throw new OperativException(TipoError.ErrorEmailYaRegistrado);
        }
    }
}

using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.SEC.Helpers;

namespace Operativ.SEC.Implementaciones;
public partial class UsuarioService
{
    public int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int? idFamilia, int? idCliente)
    {
        ValidarUnicidad(nombreUsuario, correoElectronico, null);
        ValidarEmpresaSegunFamilia(idFamilia, idCliente);

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
            Salt = salt,
            IdCliente = idCliente
        };

        int idUsuario = usuarioRepositorio.Insertar(usuario);

        if (idFamilia.HasValue)
        {
            usuarioRepositorio.AsignarFamilia(idUsuario, idFamilia.Value);
        }

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.AltaUsuario);

        return idUsuario;
    }

    public void ModificarUsuario(Usuario usuario, int? idFamilia, int? idCliente)
    {
        ValidarUnicidad(usuario.NombreUsuario, usuario.Email, usuario.IdUsuario);
        ValidarEmpresaSegunFamilia(idFamilia, idCliente);

        usuario.IdCliente = idCliente;
        usuarioRepositorio.Modificar(usuario);

        usuarioRepositorio.QuitarFamilias(usuario.IdUsuario);

        if (idFamilia.HasValue)
        {
            usuarioRepositorio.AsignarFamilia(usuario.IdUsuario, idFamilia.Value);
        }

        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.ModificacionUsuario);
    }

    public void BajaUsuario(int idUsuario)
    {
        ValidarNoEsUltimoUsuarioDeFamilia(idUsuario);

        usuarioRepositorio.BajaLogica(idUsuario);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.BajaUsuario);
    }

    public void AsignarEmpresa(int idUsuario, int idCliente)
    {
        Usuario usuario = usuarioRepositorio.GetPorId(idUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);

        usuario.IdCliente = idCliente;
        usuarioRepositorio.Modificar(usuario);
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

    public List<Usuario> ListarUsuariosClienteSinEmpresa()
    {
        Familia familiaCliente = familiaRepositorio.GetPorNombre(NombreFamilia.Cliente);

        if (familiaCliente == null)
        {
            return new List<Usuario>();
        }

        return usuarioRepositorio.ListarSinEmpresaPorFamilia(familiaCliente.IdFamilia);
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

    private void ValidarEmpresaSegunFamilia(int? idFamilia, int? idCliente)
    {
        if (!idCliente.HasValue)
        {
            return;
        }

        if (!EsFamiliaCliente(idFamilia))
        {
            throw new OperativException(TipoError.ErrorUsuarioClienteSinEmpresa);
        }
    }

    private bool EsFamiliaCliente(int? idFamilia)
    {
        if (!idFamilia.HasValue)
        {
            return false;
        }

        Familia familia = familiaRepositorio.GetPorId(idFamilia.Value);

        if (familia == null)
        {
            return false;
        }

        return familia.Nombre == NombreFamilia.Cliente;
    }

    private void ValidarNoEsUltimoUsuarioDeFamilia(int idUsuario)
    {
        List<Familia> familias = familiaRepositorio.GetFamiliasDeUsuario(idUsuario);

        foreach (Familia familia in familias)
        {
            if (!usuarioRepositorio.ExisteOtroUsuarioActivoEnFamilia(familia.IdFamilia, idUsuario))
            {
                throw new OperativException(TipoError.ErrorUltimoUsuarioDeFamilia, new string[] { familia.Nombre });
            }
        }
    }
}

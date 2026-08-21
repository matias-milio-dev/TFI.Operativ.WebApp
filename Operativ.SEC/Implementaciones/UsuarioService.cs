using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Helpers;

namespace Operativ.SEC.Implementaciones;
public partial class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepositorio usuarioRepositorio;
    private readonly IFamiliaRepositorio familiaRepositorio;
    private readonly IBitacoraService bitacoraService;

    public UsuarioService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();
        familiaRepositorio = fabricaRepositorio.CrearFamiliaRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public void RecuperarContrasena(string nombreUsuario)
    {
        Usuario usuario = GetUsuarioExistente(nombreUsuario);
        string contrasenaTemporal = ClaveHelper.GenerarContrasenaTemporal();
        string nuevoSalt = HashHelper.GenerarSalt();
        string nuevoHash = HashHelper.GenerarHash(contrasenaTemporal, nuevoSalt);

        EmailHelper.EnviarContrasenaTemporal(usuario.Email, usuario.NombreUsuario, contrasenaTemporal);
        usuarioRepositorio.ActualizarContrasena(usuario.IdUsuario, nuevoHash, nuevoSalt);
        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.RecuperacionContrasena);
    }

    public void CambiarClave(int idUsuario, string claveActual, string claveNueva)
    {
        Usuario usuario = usuarioRepositorio.GetPorId(idUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);

        bool claveActualValida = HashHelper.ValidarContrasena(claveActual, usuario.Salt, usuario.Contrasena);

        if (!claveActualValida)
        {
            throw new OperativException(TipoError.ErrorContrasenaActualIncorrecta);
        }

        if (!ClaveHelper.EsCompleja(claveNueva))
        {
            throw new OperativException(TipoError.ErrorClaveNoCumpleComplejidad);
        }

        string nuevoSalt = HashHelper.GenerarSalt();
        string nuevoHash = HashHelper.GenerarHash(claveNueva, nuevoSalt);

        usuarioRepositorio.ActualizarContrasena(idUsuario, nuevoHash, nuevoSalt);
        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.CambioClave);
    }

    public void DesbloquearUsuario(int idUsuario)
    {
        usuarioRepositorio.Desbloquear(idUsuario);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.DesbloqueoUsuario);
    }

    private Usuario GetUsuarioExistente(string nombreUsuario)
    {
        Usuario usuario = usuarioRepositorio.GetPorNombreUsuario(nombreUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);
        return usuario;
    }
}

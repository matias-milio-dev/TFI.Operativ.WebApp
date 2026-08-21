using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.BE.Modelos.Composite;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Helpers;

namespace Operativ.SEC.Implementaciones.Estrategias;
public class LoginNormalStrategy : ILoginStrategy
{
    private readonly IUsuarioRepositorio usuarioRepositorio;
    private readonly IFamiliaService familiaService;
    private readonly IBitacoraService bitacoraService;

    public LoginNormalStrategy()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        familiaService = fabricaSeguridad.CrearFamiliaService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public ResultadoAutenticacion Autenticar(string nombreUsuario, string contrasena)
    {
        Usuario usuario = ValidarCredenciales(nombreUsuario, contrasena);

        Familia perfil = familiaService.GetPerfilDeUsuario(usuario.IdUsuario);
        FamiliaCompuesto arbolPermisos = familiaService.ArmarArbolPermisos(usuario.IdUsuario);

        return new ResultadoAutenticacion
        {
            Usuario = usuario,
            Perfil = perfil,
            ArbolPermisos = arbolPermisos
        };
    }

    private Usuario ValidarCredenciales(string nombreUsuario, string contrasena)
    {
        Usuario usuario = usuarioRepositorio.GetPorNombreUsuario(nombreUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);

        if (usuario.Bloqueado)
        {
            throw new OperativException(TipoError.ErrorUsuarioBloqueado, new string[] { usuario.NombreUsuario });
        }

        bool contrasenaValida = HashHelper.ValidarContrasena(contrasena, usuario.Salt, usuario.Contrasena);

        if (!contrasenaValida)
        {
            ManejarIntentoFallido(usuario);
        }

        usuarioRepositorio.ResetearIntentosFallidos(usuario.IdUsuario);
        usuario.IntentosFallidos = 0;

        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.LoginExitoso);

        return usuario;
    }

    private void ManejarIntentoFallido(Usuario usuario)
    {
        int intentosFallidos = usuario.IntentosFallidos + 1;
        bool bloqueado = intentosFallidos >= ConfiguracionAplicacion.IntentosMaximosLogin;

        usuarioRepositorio.ActualizarIntentosFallidos(usuario.IdUsuario, intentosFallidos, bloqueado);

        if (bloqueado)
        {
            bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.LoginBloqueado);
            throw new OperativException(TipoError.ErrorUsuarioBloqueado, new string[] { usuario.NombreUsuario });
        }

        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.IntentoLoginFallido);
        int intentosRestantes = ConfiguracionAplicacion.IntentosMaximosLogin - intentosFallidos;
        throw new OperativException(TipoError.ErrorContrasenaIncorrecta, new string[] { intentosRestantes.ToString() });
    }
}

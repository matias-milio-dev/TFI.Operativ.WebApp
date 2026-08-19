using System;
using System.Text;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
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

    public Usuario ValidarCredenciales(string nombreUsuario, string contrasena)
    {
        Usuario usuario = GetUsuarioExistente(nombreUsuario);

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

    public void RecuperarContrasena(string nombreUsuario)
    {
        Usuario usuario = GetUsuarioExistente(nombreUsuario);
        string contrasenaTemporal = GenerarContrasenaTemporal();
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

    private string GenerarContrasenaTemporal()
    {
        Random generadorAleatorio = new Random();
        string candidato;

        do
        {
            candidato = GenerarCandidatoContrasenaTemporal(generadorAleatorio);
        }
        while (!ClaveHelper.EsCompleja(candidato));

        return candidato;
    }

    private string GenerarCandidatoContrasenaTemporal(Random generadorAleatorio)
    {
        string caracteresValidos = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        StringBuilder resultado = new StringBuilder();

        for (int indice = 0; indice < ConfiguracionAplicacion.LongitudContrasenaTemporal; indice++)
        {
            int posicion = generadorAleatorio.Next(caracteresValidos.Length);
            resultado.Append(caracteresValidos[posicion]);
        }

        return resultado.ToString();
    }
}

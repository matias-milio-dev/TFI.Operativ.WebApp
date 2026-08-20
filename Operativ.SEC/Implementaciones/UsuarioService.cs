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

//Clase parcial de UsuarioService que maneja todo lo relacionado a la autenticacion del usuario
//y gestion de claves
public partial class UsuarioService : IUsuarioService
{
    //Miembros de clase
    private readonly IUsuarioRepositorio usuarioRepositorio;
    private readonly IFamiliaRepositorio familiaRepositorio;
    private readonly IBitacoraService bitacoraService;

    //Inicializacion con las fabrica de seguridad y repositorio
    public UsuarioService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();
        familiaRepositorio = fabricaRepositorio.CrearFamiliaRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    //Primero se obtiene el objeto usuario por su username unico y se valida que no este bloqueado
    //De lo contrario se arroja una excepcion de negocio tipada por enumerable de errores propio.
    //Se valida la contrasena ingresada encriptandola con el mismo algoritmo que la almacenada y devuelve
    //un bool que se verifica> De ser correcto se resetea los ingresos erroneos en la base, registra en bitacora 
    //y devuelve el usuario que se autentico, de lo contrario> procede a invocar el metodo privado que manejar el intento fallido.
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

    //Metodo que genera una nueva contresena temporal aleatoria y envia por smtp al mail de la base de datos
    //posteriormente actualiza la tabla usuario y registra en bitacora.
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

    //Metodo para cambiar la clave del usuario manualmente con validaciones por compejidad.
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

    //Desbloquea un usuario en la base de datos
    public void DesbloquearUsuario(int idUsuario)
    {
        usuarioRepositorio.Desbloquear(idUsuario);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.DesbloqueoUsuario);
    }

    //Obtiene el usuario por su nombre unico (ingresado previamente en la UI)
    private Usuario GetUsuarioExistente(string nombreUsuario)
    {
        Usuario usuario = usuarioRepositorio.GetPorNombreUsuario(nombreUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);
        return usuario;
    }

    //Incrementa en uno el contador de intentos fallidos que vino de la base de datos para este usuario
    //Si los intentos fallidos superan los intentos maximos (configurables por la clase ConfiguracionAplicacion)
    //El usuario se actualiza en bd con el flag bloqueado,se registra en bitacora y se arroja una excepcion de negocio.
    //Caso contrario se registra el ingreso fallido en bitacora y se muestra una excepcion con los intentos restantes.
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

    //Genera una contrasena temporal con un random
    //Con un bucle do while con las condiciones de complejidad establecidas en ClaveHelper.cs
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

    //Algoritmo de generacion de contrasenas con stringbuilder consultando ConfiguracionAplicacion 
    //Para ver la longitud requerida.
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

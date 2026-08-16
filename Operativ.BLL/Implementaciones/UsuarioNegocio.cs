using System;
using System.Text;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BLL.Configuracion;
using Operativ.BLL.Contratos;
using Operativ.BLL.Errores;
using Operativ.BLL.Fabricas;
using Operativ.BLL.Helpers;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Helpers;

namespace Operativ.BLL.Implementaciones
{
    public class UsuarioNegocio : IUsuarioNegocio
    {
        private readonly IUsuarioRepositorio usuarioRepositorio;
        private readonly IBitacoraNegocio bitacoraNegocio;

        public UsuarioNegocio()
        {
            FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
            usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();

            FabricaNegocio fabricaNegocio = new FabricaNegocio();
            bitacoraNegocio = fabricaNegocio.CrearBitacoraNegocio();
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

            bitacoraNegocio.Registrar(usuario.IdUsuario, TipoAccionBitacora.LoginExitoso);

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
            bitacoraNegocio.Registrar(usuario.IdUsuario, TipoAccionBitacora.RecuperacionContrasena);
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
                bitacoraNegocio.Registrar(usuario.IdUsuario, TipoAccionBitacora.LoginBloqueado);
                throw new OperativException(TipoError.ErrorUsuarioBloqueado, new string[] { usuario.NombreUsuario });
            }

            int intentosRestantes = ConfiguracionAplicacion.IntentosMaximosLogin - intentosFallidos;
            throw new OperativException(TipoError.ErrorContrasenaIncorrecta, new string[] { intentosRestantes.ToString() });
        }

        private string GenerarContrasenaTemporal()
        {
            string caracteresValidos = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            StringBuilder resultado = new StringBuilder();
            Random generadorAleatorio = new Random();

            for (int indice = 0; indice < ConfiguracionAplicacion.LongitudContrasenaTemporal; indice++)
            {
                int posicion = generadorAleatorio.Next(caracteresValidos.Length);
                resultado.Append(caracteresValidos[posicion]);
            }

            return resultado.ToString();
        }
    }
}

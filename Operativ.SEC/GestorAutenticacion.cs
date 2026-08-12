using Operativ.BE;
using Operativ.Comun;
using Operativ.DAL;

namespace Operativ.SEC
{
    public static class GestorAutenticacion
    {
        private static readonly IUsuarioDAL _usuarioDAL = FabricaDAL.Instancia.CrearUsuarioDAL();

        public static Usuario ValidarCredenciales(string nombreUsuario, string clave)
        {
            Usuario usuario = _usuarioDAL.ObtenerPorNombreUsuario(nombreUsuario);

            if (usuario == null)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorUsuarioOClaveIncorrectos);
            }

            if (!usuario.Activo)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorUsuarioInactivo);
            }

            if (usuario.Bloqueado)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorUsuarioBloqueado);
            }

            bool claveValida = HashHelper.VerificarClave(clave, usuario.ClaveHash, usuario.ClaveSalt);

            if (!claveValida)
            {
                ResultadoIntentoLogin resultado = _usuarioDAL.RegistrarIntentoFallido(usuario.IdUsuario, ConfiguracionAplicacion.IntentosMaximosLogin);
                throw new ExcepcionNegocio(resultado.Bloqueado ? CodigosError.ErrorUsuarioBloqueado : CodigosError.ErrorIntentoFallidoLogin);
            }

            _usuarioDAL.RegistrarLoginExitoso(usuario.IdUsuario);

            return usuario;
        }

        public static void IniciarSesion(Usuario usuario)
        {
            ContextoSesion.Actual.IniciarSesion(usuario, usuario.Familias, usuario.PatentesEfectivas);
        }

        public static void CerrarSesion()
        {
            ContextoSesion.Actual.CerrarSesion();
        }
    }
}

using Operativ.BE;
using Operativ.Comun;
using Operativ.DAL;

namespace Operativ.SEC
{
    public static class GestorRecuperacionClave
    {
        private static readonly IUsuarioDAL _usuarioDAL = FabricaDAL.Instancia.CrearUsuarioDAL();

        public static string GenerarNuevaClaveTemporal(int idUsuario)
        {
            string claveTemporal = HashHelper.GenerarClaveTemporal();
            byte[] salt = HashHelper.GenerarSalt();
            byte[] hash = HashHelper.CalcularHash(claveTemporal, salt);

            _usuarioDAL.CambiarClave(idUsuario, hash, salt, claveTemporal: true);

            return claveTemporal;
        }

        public static void CambiarClave(int idUsuario, string claveNueva)
        {
            ValidadorReglasClave.Validar(claveNueva);

            byte[] salt = HashHelper.GenerarSalt();
            byte[] hash = HashHelper.CalcularHash(claveNueva, salt);

            _usuarioDAL.CambiarClave(idUsuario, hash, salt, claveTemporal: false);
        }
    }
}

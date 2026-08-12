using Operativ.Comun;

namespace Operativ.SEC
{
    public static class GestorAutorizacion
    {
        public static void RequerirSesionActiva()
        {
            if (!ContextoSesion.Actual.EstaAutenticado)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorSesionExpirada);
            }
        }

        public static void RequerirPatente(string codigoPatente)
        {
            RequerirSesionActiva();

            if (!ContextoSesion.Actual.TienePatente(codigoPatente))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorAccesoDenegado);
            }
        }

        public static bool TienePatente(string codigoPatente)
        {
            return ContextoSesion.Actual.EstaAutenticado && ContextoSesion.Actual.TienePatente(codigoPatente);
        }
    }
}

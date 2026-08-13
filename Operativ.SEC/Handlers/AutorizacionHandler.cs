using System;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Handlers
{
    public class AutorizacionHandler
    {
        private readonly SesionHandler sesionHandler;

        public AutorizacionHandler()
        {
            sesionHandler = new SesionHandler();
        }

        public bool EsPerfil(string nombreFamilia)
        {
            Familia perfil = sesionHandler.GetPerfil();

            if (perfil == null)
            {
                return false;
            }

            return string.Equals(perfil.Nombre, nombreFamilia, StringComparison.OrdinalIgnoreCase);
        }

        public string GetNombrePerfil()
        {
            Familia perfil = sesionHandler.GetPerfil();

            if (perfil == null)
            {
                return null;
            }

            return perfil.Nombre;
        }
    }
}

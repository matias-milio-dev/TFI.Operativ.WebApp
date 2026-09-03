using System;
using Operativ.BE.Entidades;
using Operativ.BE.Modelos.Composite;

namespace Operativ.SEC.Handlers;
public class AutorizacionHandler
{
    private readonly SesionHandler sesionHandler;
    public AutorizacionHandler()
    {
        sesionHandler = new SesionHandler();
    }

    public bool EsAlgunPerfil(string[] nombresFamilia)
    {
        Familia perfil = sesionHandler.GetPerfil();

        if (perfil == null)
        {
            return false;
        }

        foreach (string nombreFamilia in nombresFamilia)
        {
            if (string.Equals(perfil.Nombre, nombreFamilia, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public bool TienePatente(string nombrePatente)
    {
        FamiliaCompuesto arbolPermisos = sesionHandler.GetArbolPermisos();

        if (arbolPermisos == null)
        {
            return false;
        }

        return arbolPermisos.ObtenerNombresPatentes().Contains(nombrePatente);
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

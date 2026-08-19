using System;
using Operativ.BE.Entidades;

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

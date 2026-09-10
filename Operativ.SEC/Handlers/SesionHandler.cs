using System.Collections.Generic;
using System.Web;
using Operativ.BE.Modelos;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Handlers;
public class SesionHandler
{
    private const string ClaveUsuario = "Operativ_UsuarioLogueado";

    private const string ClavePerfil = "Operativ_PerfilLogueado";

    private const string ClaveArbolPermisos = "Operativ_ArbolPermisosLogueado";

    private const string ClaveFallasIntegridad = "Operativ_FallasIntegridad";

    public void IniciarSesion(Usuario usuario, Familia perfil, FamiliaCompuesto arbolPermisos)
    {
        HttpContext.Current.Session[ClaveUsuario] = usuario;
        HttpContext.Current.Session[ClavePerfil] = perfil;
        HttpContext.Current.Session[ClaveArbolPermisos] = arbolPermisos;
    }

    public Usuario GetUsuario()
    {
        return HttpContext.Current.Session[ClaveUsuario] as Usuario;
    }

    public Familia GetPerfil()
    {
        return HttpContext.Current.Session[ClavePerfil] as Familia;
    }

    public FamiliaCompuesto GetArbolPermisos()
    {
        return HttpContext.Current.Session[ClaveArbolPermisos] as FamiliaCompuesto;
    }

    public void GuardarFallasIntegridad(List<ResultadoVerificacionTabla> fallas)
    {
        HttpContext.Current.Session[ClaveFallasIntegridad] = fallas;
    }

    public List<ResultadoVerificacionTabla> GetFallasIntegridad()
    {
        return HttpContext.Current.Session[ClaveFallasIntegridad] as List<ResultadoVerificacionTabla>;
    }

    public bool HaySesionActiva()
    {
        return GetUsuario() != null;
    }

    public void CerrarSesion()
    {
        HttpContext.Current.Session.Clear();
        HttpContext.Current.Session.Abandon();
    }
}

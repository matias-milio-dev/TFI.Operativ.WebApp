using System.Web;
using Operativ.BE.Composite;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Handlers;
public class SesionHandler
{
    private const string ClaveUsuario = "Operativ_UsuarioLogueado";

    private const string ClavePerfil = "Operativ_PerfilLogueado";

    private const string ClaveArbolPermisos = "Operativ_ArbolPermisosLogueado";

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

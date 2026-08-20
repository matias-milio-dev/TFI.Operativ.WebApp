using Operativ.SEC.Handlers;
using Operativ.Web.Controles;
using Operativ.Web.Master;

namespace Operativ.Web.Paginas;
public abstract class PaginaSeguraBase : PaginaBase
{
    protected SesionHandler SesionHandler { get; private set; }
    protected AutorizacionHandler AutorizacionHandler { get; private set; }
    protected abstract string[] PerfilesPermitidos { get; }

    protected Notificaciones ControlNotificaciones
    {
        get { return ((Principal)Master).ControlNotificaciones; }
    }

    protected override void OnInit(System.EventArgs e)
    {
        base.OnInit(e);

        SesionHandler = new SesionHandler();
        AutorizacionHandler = new AutorizacionHandler();

        ValidarAcceso();
    }

    private void ValidarAcceso()
    {
        if (!SesionHandler.HaySesionActiva())
        {
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?err=sesion");
        }

        if (!AutorizacionHandler.EsAlgunPerfil(PerfilesPermitidos))
        {
            Response.Redirect("~/Paginas/Comun/NoAutorizado.aspx");
        }
    }
}

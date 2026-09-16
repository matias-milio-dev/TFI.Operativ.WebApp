using System;
using System.IO;
using System.Web.UI;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web.Controles;
public partial class MenuPublico : UserControl
{
    private const string ClaseLinkActivo = "menu-publico-link menu-publico-link-activo";

    protected void Page_Load(object sender, EventArgs e)
    {
        SesionHandler sesionHandler = new SesionHandler();
        string nombrePerfil = sesionHandler.HaySesionActiva() ? sesionHandler.GetPerfil()?.Nombre : null;

        lnkInicio.NavigateUrl = nombrePerfil == null
            ? "~/Paginas/Publico/Servicios.aspx"
            : NavegacionHelper.ObtenerUrlHome(nombrePerfil);

        MarcarActivo();
    }

    private void MarcarActivo()
    {
        string paginaActual = Path.GetFileName(Request.AppRelativeCurrentExecutionFilePath);

        if (string.Equals(paginaActual, "QuienesSomos.aspx", StringComparison.OrdinalIgnoreCase))
        {
            lnkQuienesSomos.CssClass = ClaseLinkActivo;
        }
        else if (string.Equals(paginaActual, "Servicios.aspx", StringComparison.OrdinalIgnoreCase))
        {
            lnkServicios.CssClass = ClaseLinkActivo;
        }
        else if (string.Equals(paginaActual, "Contacto.aspx", StringComparison.OrdinalIgnoreCase))
        {
            lnkContacto.CssClass = ClaseLinkActivo;
        }
        else
        {
            lnkInicio.CssClass = ClaseLinkActivo;
        }
    }
}

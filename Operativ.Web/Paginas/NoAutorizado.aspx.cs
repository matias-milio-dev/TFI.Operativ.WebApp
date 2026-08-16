using System;
using Operativ.SEC.Handlers;

namespace Operativ.Web.Paginas
{
    public partial class NoAutorizado : PaginaBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SesionHandler sesionHandler = new SesionHandler();

            if (sesionHandler.HaySesionActiva())
            {
                string nombrePerfil = sesionHandler.GetPerfil().Nombre;
                lnkVolverHome.NavigateUrl = ResolveUrl(NavegacionHelper.ObtenerUrlHome(nombrePerfil));
            }
            else
            {
                lnkVolverHome.NavigateUrl = ResolveUrl("~/Login.aspx");
            }
        }
    }
}

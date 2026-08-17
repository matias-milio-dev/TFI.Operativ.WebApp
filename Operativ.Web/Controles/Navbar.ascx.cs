using System;
using System.Web.UI;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web.Controles
{
    public partial class Navbar : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AutorizacionHandler autorizacionHandler = new AutorizacionHandler();
            string nombrePerfil = autorizacionHandler.GetNombrePerfil();

            if (string.IsNullOrEmpty(nombrePerfil))
            {
                lnkHome.Visible = false;
                return;
            }

            lnkHome.NavigateUrl = ResolveUrl(NavegacionHelper.ObtenerUrlHome(nombrePerfil));
            lnkUsuarios.Visible = string.Equals(nombrePerfil, NavegacionHelper.PerfilAdministrador, StringComparison.Ordinal);
        }
    }
}

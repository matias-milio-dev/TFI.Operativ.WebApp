using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.SEC;
using Operativ.Web.Controles;

namespace Operativ.Web
{
    public partial class Default : PaginaBase
    {
        protected Literal litBienvenida;
        protected DashboardResumen ucDashboard;

        protected void Page_Load(object sender, EventArgs e)
        {
            var usuario = ContextoSesion.Actual.UsuarioActual;
            litBienvenida.Text = $"{(string)GetGlobalResourceObject("Textos", "MenuInicio")} - {usuario.NombreCompleto}";

            if (GestorAutorizacion.TienePatente("MONITOREO_DASHBOARD"))
            {
                ucDashboard.CargarIndicadores(null);
            }
            else
            {
                ucDashboard.Visible = false;
            }
        }
    }
}

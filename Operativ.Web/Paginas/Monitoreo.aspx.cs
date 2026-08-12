using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.Web.Controles;

namespace Operativ.Web.Paginas
{
    public partial class Monitoreo : PaginaBase
    {
        protected override string PatenteRequerida => "MONITOREO_DASHBOARD";

        protected Literal litTitulo;
        protected DashboardResumen ucDashboard;

        protected void Page_Load(object sender, EventArgs e)
        {
            litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuMonitoreo");
            ucDashboard.CargarIndicadores(null);
        }
    }
}

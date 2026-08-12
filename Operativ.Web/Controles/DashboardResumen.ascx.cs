using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;

namespace Operativ.Web.Controles
{
    public partial class DashboardResumen : UserControl
    {
        private readonly IMonitoreoBLL _monitoreoBLL = FabricaBLL.Instancia.CrearMonitoreoBLL();

        protected Literal litActivos;
        protected Literal litIncidentesAbiertos;
        protected Literal litSuscripcionesActivas;
        protected Literal litAlertasUrgentes;
        protected Literal litEtiquetaActivos;
        protected Literal litEtiquetaIncidentes;
        protected Literal litEtiquetaSuscripciones;
        protected Literal litEtiquetaAlertas;

        protected void Page_Load(object sender, EventArgs e)
        {
            litEtiquetaActivos.Text = (string)GetGlobalResourceObject("Textos", "DashboardActivos");
            litEtiquetaIncidentes.Text = (string)GetGlobalResourceObject("Textos", "DashboardIncidentesAbiertos");
            litEtiquetaSuscripciones.Text = (string)GetGlobalResourceObject("Textos", "DashboardSuscripcionesActivas");
            litEtiquetaAlertas.Text = (string)GetGlobalResourceObject("Textos", "DashboardAlertasUrgentes");
        }

        public void CargarIndicadores(int? idCliente)
        {
            var fila = _monitoreoBLL.ObtenerIndicadores(idCliente);
            if (fila == null) return;

            litActivos.Text = fila["ActivosActivos"].ToString();
            litIncidentesAbiertos.Text = fila["IncidentesAbiertos"].ToString();
            litSuscripcionesActivas.Text = fila["SuscripcionesActivas"].ToString();
            litAlertasUrgentes.Text = fila["AlertasUrgentes"].ToString();
        }
    }
}

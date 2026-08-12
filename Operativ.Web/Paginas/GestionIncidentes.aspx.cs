using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.SEC;

namespace Operativ.Web.Paginas
{
    public partial class GestionIncidentes : PaginaBase
    {
        protected override string PatenteRequerida => "INCIDENTE_CONSULTAR";

        private readonly IClienteBLL _clienteBLL = FabricaBLL.Instancia.CrearClienteBLL();
        private readonly IActivoBLL _activoBLL = FabricaBLL.Instancia.CrearActivoBLL();
        private readonly IIncidenteBLL _incidenteBLL = FabricaBLL.Instancia.CrearIncidenteBLL();

        protected Literal litTitulo;
        protected DropDownList ddlCliente;
        protected DropDownList ddlActivo;
        protected GridView gvIncidentes;
        protected Panel pnlFormulario;
        protected DropDownList ddlCategoria;
        protected DropDownList ddlPrioridad;
        protected TextBox txtDescripcion;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuIncidentes");
                ddlCliente.DataSource = _clienteBLL.Listar(null, 1, 500);
                ddlCliente.DataTextField = "RazonSocial";
                ddlCliente.DataValueField = "IdCliente";
                ddlCliente.DataBind();

                CargarActivosDelCliente();
                CargarIncidentes();
            }
        }

        private void CargarActivosDelCliente()
        {
            int idCliente = Convert.ToInt32(ddlCliente.SelectedValue);
            ddlActivo.DataSource = _activoBLL.ListarPorCliente(idCliente);
            ddlActivo.DataTextField = "Nombre";
            ddlActivo.DataValueField = "IdActivo";
            ddlActivo.DataBind();
            pnlFormulario.Visible = ddlActivo.Items.Count > 0;
        }

        private void CargarIncidentes()
        {
            if (ddlActivo.Items.Count == 0)
            {
                gvIncidentes.DataSource = null;
                gvIncidentes.DataBind();
                return;
            }

            int idActivo = Convert.ToInt32(ddlActivo.SelectedValue);
            gvIncidentes.DataSource = _incidenteBLL.ListarPorActivo(idActivo);
            gvIncidentes.DataBind();
        }

        protected void ddlCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarActivosDelCliente();
            CargarIncidentes();
        }

        protected void ddlActivo_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarIncidentes();
        }

        protected void gvIncidentes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Cerrar") return;

            int idIncidente = Convert.ToInt32(gvIncidentes.DataKeys[Convert.ToInt32(e.CommandArgument)].Value);
            try
            {
                GestorAutorizacion.RequerirPatente("INCIDENTE_ALTA");
                _incidenteBLL.Cerrar(idIncidente);
                CargarIncidentes();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid
                || ddlActivo.Items.Count == 0)
            {
                return;
            }

            try
            {
                GestorAutorizacion.RequerirPatente("INCIDENTE_ALTA");
                int idActivo = Convert.ToInt32(ddlActivo.SelectedValue);
                _incidenteBLL.Alta(idActivo, txtDescripcion.Text.Trim(), ddlPrioridad.SelectedValue, ddlCategoria.SelectedValue);

                ((Master.SiteMaster)Master).MostrarExito("Incidente registrado correctamente (XML generado por IncidentesService).");
                txtDescripcion.Text = string.Empty;
                CargarIncidentes();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }
    }
}

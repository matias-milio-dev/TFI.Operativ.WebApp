using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;

namespace Operativ.Web.Paginas
{
    public partial class GestionActivos : PaginaBase
    {
        protected override string PatenteRequerida => "ACTIVO_ABM";

        private readonly IClienteBLL _clienteBLL = FabricaBLL.Instancia.CrearClienteBLL();
        private readonly IActivoBLL _activoBLL = FabricaBLL.Instancia.CrearActivoBLL();
        private readonly ISuscripcionBLL _suscripcionBLL = FabricaBLL.Instancia.CrearSuscripcionBLL();

        protected Literal litTitulo;
        protected DropDownList ddlCliente;
        protected GridView gvActivos;
        protected Panel pnlFormulario;
        protected DropDownList ddlSuscripcion;
        protected TextBox txtNombre;
        protected DropDownList ddlTipoActivo;
        protected TextBox txtIdentificador;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuActivos");
                ddlCliente.DataSource = _clienteBLL.Listar(null, 1, 500);
                ddlCliente.DataTextField = "RazonSocial";
                ddlCliente.DataValueField = "IdCliente";
                ddlCliente.DataBind();

                CargarActivos();
                CargarSuscripcionesActivas();
            }
        }

        private int IdClienteSeleccionado => Convert.ToInt32(ddlCliente.SelectedValue);

        private void CargarActivos()
        {
            gvActivos.DataSource = _activoBLL.ListarPorCliente(IdClienteSeleccionado);
            gvActivos.DataBind();
        }

        private void CargarSuscripcionesActivas()
        {
            DataTable suscripciones = _suscripcionBLL.ListarPorCliente(IdClienteSeleccionado);
            DataView vista = suscripciones.DefaultView;
            vista.RowFilter = "CodigoEstado = 'ACTIVA'";

            ddlSuscripcion.DataSource = vista;
            ddlSuscripcion.DataTextField = "NombrePaquete";
            ddlSuscripcion.DataValueField = "IdSuscripcion";
            ddlSuscripcion.DataBind();

            pnlFormulario.Visible = ddlSuscripcion.Items.Count > 0;
        }

        protected void ddlCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarActivos();
            CargarSuscripcionesActivas();
        }

        protected void gvActivos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Baja") return;

            int idActivo = Convert.ToInt32(gvActivos.DataKeys[Convert.ToInt32(e.CommandArgument)].Value);
            try
            {
                _activoBLL.Baja(idActivo);
                CargarActivos();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid
                || ddlSuscripcion.Items.Count == 0)
            {
                return;
            }

            try
            {
                _activoBLL.Alta(IdClienteSeleccionado, Convert.ToInt32(ddlSuscripcion.SelectedValue),
                    txtNombre.Text.Trim(), ddlTipoActivo.SelectedValue, txtIdentificador.Text.Trim());

                ((Master.SiteMaster)Master).MostrarExito("Activo dado de alta correctamente.");
                txtNombre.Text = string.Empty;
                txtIdentificador.Text = string.Empty;
                CargarActivos();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }
    }
}

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.SEC;

namespace Operativ.Web.Paginas
{
    public partial class GestionSuscripcionesCliente : PaginaBase
    {
        protected override string PatenteRequerida => "SUSCRIPCION_ABM";

        private readonly IClienteBLL _clienteBLL = FabricaBLL.Instancia.CrearClienteBLL();
        private readonly IPaqueteBLL _paqueteBLL = FabricaBLL.Instancia.CrearPaqueteBLL();
        private readonly ISuscripcionBLL _suscripcionBLL = FabricaBLL.Instancia.CrearSuscripcionBLL();

        protected Literal litTitulo;
        protected Panel pnlResumen;
        protected Literal litResumen;
        protected DropDownList ddlCliente;
        protected DropDownList ddlPaquete;
        protected TextBox txtMeses;
        protected GridView gvSuscripciones;
        protected Panel pnlPago;
        protected Literal litIdSuscripcionPago;
        protected HiddenField hdnIdSuscripcionPago;
        protected DropDownList ddlMedioPago;
        protected TextBox txtMontoPago;
        protected TextBox txtReferenciaPago;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuSuscripciones");
                CargarCombos();
                CargarSuscripciones();
            }
        }

        private void CargarCombos()
        {
            ddlCliente.DataSource = _clienteBLL.Listar(null, 1, 500);
            ddlCliente.DataTextField = "RazonSocial";
            ddlCliente.DataValueField = "IdCliente";
            ddlCliente.DataBind();

            ddlPaquete.DataSource = _paqueteBLL.Listar(soloActivos: true);
            ddlPaquete.DataTextField = "Nombre";
            ddlPaquete.DataValueField = "IdPaquete";
            ddlPaquete.DataBind();
        }

        private int? IdClienteSeleccionado => ddlCliente.Items.Count > 0 ? (int?)Convert.ToInt32(ddlCliente.SelectedValue) : null;

        private void CargarSuscripciones()
        {
            if (IdClienteSeleccionado == null)
            {
                gvSuscripciones.DataSource = null;
                gvSuscripciones.DataBind();
                return;
            }

            gvSuscripciones.DataSource = _suscripcionBLL.ListarPorCliente(IdClienteSeleccionado.Value);
            gvSuscripciones.DataBind();
        }

        protected void ddlCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarSuscripciones();
        }

        protected void btnGenerarResumen_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid
                || IdClienteSeleccionado == null)
            {
                return;
            }

            try
            {
                Cliente cliente = _clienteBLL.Obtener(IdClienteSeleccionado.Value);
                int idPaquete = Convert.ToInt32(ddlPaquete.SelectedValue);

                var resumen = _suscripcionBLL.GenerarResumen(cliente.Cuit, cliente.RazonSocial, cliente.CorreoElectronico, idPaquete);

                litResumen.Text = resumen.ResumenHtml;
                pnlResumen.Visible = true;
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }

        protected void btnConfirmarAlta_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid
                || IdClienteSeleccionado == null)
            {
                return;
            }

            try
            {
                int idPaquete = Convert.ToInt32(ddlPaquete.SelectedValue);
                int meses = Convert.ToInt32(txtMeses.Text);

                _suscripcionBLL.Alta(IdClienteSeleccionado.Value, idPaquete, meses);

                ((Master.SiteMaster)Master).MostrarExito("Suscripción creada correctamente.");
                pnlResumen.Visible = false;
                CargarSuscripciones();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }

        protected void gvSuscripciones_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idSuscripcion = Convert.ToInt32(gvSuscripciones.DataKeys[Convert.ToInt32(e.CommandArgument)].Value);

            if (e.CommandName == "Cancelar")
            {
                try
                {
                    _suscripcionBLL.Cancelar(idSuscripcion);
                    CargarSuscripciones();
                }
                catch (ExcepcionNegocio excepcionNegocio)
                {
                    ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
                }
            }
            else if (e.CommandName == "Pagar")
            {
                hdnIdSuscripcionPago.Value = idSuscripcion.ToString();
                litIdSuscripcionPago.Text = idSuscripcion.ToString();
                pnlPago.Visible = true;
            }
        }

        protected void btnRegistrarPago_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMontoPago.Text)
                || !decimal.TryParse(txtMontoPago.Text, out decimal monto))
            {
                ((Master.SiteMaster)Master).MostrarMensaje(CodigosError.ErrorFormatoDatoInvalido);
                return;
            }

            try
            {
                GestorAutorizacion.RequerirPatente("SUSCRIPCION_PAGAR");
                int idSuscripcion = Convert.ToInt32(hdnIdSuscripcionPago.Value);
                _suscripcionBLL.RegistrarPago(idSuscripcion, ddlMedioPago.SelectedValue, monto, txtReferenciaPago.Text.Trim());

                ((Master.SiteMaster)Master).MostrarExito("Pago registrado correctamente.");
                pnlPago.Visible = false;
                txtMontoPago.Text = string.Empty;
                txtReferenciaPago.Text = string.Empty;
                CargarSuscripciones();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }
    }
}

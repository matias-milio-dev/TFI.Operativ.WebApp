using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;

namespace Operativ.Web.Paginas
{
    public partial class GestionClientes : PaginaBase
    {
        protected override string PatenteRequerida => "CLIENTE_ABM";

        private readonly IClienteBLL _clienteBLL = FabricaBLL.Instancia.CrearClienteBLL();

        protected Literal litTitulo;
        protected TextBox txtFiltro;
        protected GridView gvClientes;
        protected Panel pnlFormulario;
        protected HiddenField hdnIdCliente;
        protected TextBox txtCuit;
        protected TextBox txtRazonSocial;
        protected TextBox txtCorreo;
        protected TextBox txtTelefono;
        protected TextBox txtDireccion;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuClientes");
                CargarGrilla();
            }
        }

        private void CargarGrilla()
        {
            gvClientes.DataSource = _clienteBLL.Listar(txtFiltro.Text.Trim(), 1, 50);
            gvClientes.DataBind();
        }

        protected void btnBuscar_Click(object sender, EventArgs e) => CargarGrilla();

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            pnlFormulario.Visible = true;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            pnlFormulario.Visible = false;
        }

        protected void gvClientes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idCliente = Convert.ToInt32(gvClientes.DataKeys[Convert.ToInt32(e.CommandArgument)].Value);

            if (e.CommandName == "Editar")
            {
                Cliente cliente = _clienteBLL.Obtener(idCliente);
                hdnIdCliente.Value = cliente.IdCliente.ToString();
                txtCuit.Text = cliente.Cuit;
                txtCuit.Enabled = false;
                txtRazonSocial.Text = cliente.RazonSocial;
                txtCorreo.Text = cliente.CorreoElectronico;
                txtTelefono.Text = cliente.Telefono;
                txtDireccion.Text = cliente.Direccion;
                pnlFormulario.Visible = true;
            }
            else if (e.CommandName == "Baja")
            {
                try
                {
                    _clienteBLL.Baja(idCliente);
                    CargarGrilla();
                }
                catch (ExcepcionNegocio excepcionNegocio)
                {
                    ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
                }
            }
        }

        private void LimpiarFormulario()
        {
            hdnIdCliente.Value = "0";
            txtCuit.Text = string.Empty;
            txtCuit.Enabled = true;
            txtRazonSocial.Text = string.Empty;
            txtCorreo.Text = string.Empty;
            txtTelefono.Text = string.Empty;
            txtDireccion.Text = string.Empty;
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int idCliente = Convert.ToInt32(hdnIdCliente.Value);
            var cliente = new Cliente
            {
                IdCliente = idCliente,
                Cuit = txtCuit.Text.Trim(),
                RazonSocial = txtRazonSocial.Text.Trim(),
                CorreoElectronico = txtCorreo.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Direccion = txtDireccion.Text.Trim()
            };

            try
            {
                if (idCliente == 0)
                {
                    _clienteBLL.Alta(cliente);
                    ((Master.SiteMaster)Master).MostrarExito("Cliente creado correctamente.");
                }
                else
                {
                    _clienteBLL.Modificar(cliente);
                    ((Master.SiteMaster)Master).MostrarExito("Cliente actualizado correctamente.");
                }

                pnlFormulario.Visible = false;
                CargarGrilla();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }
    }
}

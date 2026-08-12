using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;

namespace Operativ.Web.Paginas
{
    public partial class GestionPaquetes : PaginaBase
    {
        protected override string PatenteRequerida => "PAQUETE_ABM";

        private readonly IPaqueteBLL _paqueteBLL = FabricaBLL.Instancia.CrearPaqueteBLL();

        protected Literal litTitulo;
        protected GridView gvPaquetes;
        protected Panel pnlFormulario;
        protected HiddenField hdnIdPaquete;
        protected TextBox txtNombre;
        protected TextBox txtPrecioBase;
        protected TextBox txtCantidadActivos;
        protected TextBox txtDescripcion;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuPaquetes");
                CargarGrilla();
            }
        }

        private void CargarGrilla()
        {
            gvPaquetes.DataSource = _paqueteBLL.Listar(soloActivos: false);
            gvPaquetes.DataBind();
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            pnlFormulario.Visible = true;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            pnlFormulario.Visible = false;
        }

        protected void gvPaquetes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idPaquete = Convert.ToInt32(gvPaquetes.DataKeys[Convert.ToInt32(e.CommandArgument)].Value);

            if (e.CommandName == "Editar")
            {
                Paquete paquete = _paqueteBLL.Obtener(idPaquete);
                hdnIdPaquete.Value = paquete.IdPaquete.ToString();
                txtNombre.Text = paquete.Nombre;
                txtPrecioBase.Text = paquete.PrecioBase.ToString(CultureInfo.InvariantCulture);
                txtCantidadActivos.Text = paquete.CantidadActivosIncluidos.ToString();
                txtDescripcion.Text = paquete.Descripcion;
                pnlFormulario.Visible = true;
            }
            else if (e.CommandName == "Baja")
            {
                try
                {
                    _paqueteBLL.Baja(idPaquete);
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
            hdnIdPaquete.Value = "0";
            txtNombre.Text = string.Empty;
            txtPrecioBase.Text = string.Empty;
            txtCantidadActivos.Text = "0";
            txtDescripcion.Text = string.Empty;
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int idPaquete = Convert.ToInt32(hdnIdPaquete.Value);
            var paquete = new Paquete
            {
                IdPaquete = idPaquete,
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                PrecioBase = decimal.Parse(txtPrecioBase.Text, CultureInfo.InvariantCulture),
                CantidadActivosIncluidos = string.IsNullOrEmpty(txtCantidadActivos.Text) ? 0 : int.Parse(txtCantidadActivos.Text)
            };

            try
            {
                if (idPaquete == 0)
                {
                    _paqueteBLL.Alta(paquete);
                    ((Master.SiteMaster)Master).MostrarExito("Paquete creado correctamente.");
                }
                else
                {
                    _paqueteBLL.Modificar(paquete);
                    ((Master.SiteMaster)Master).MostrarExito("Paquete actualizado correctamente.");
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

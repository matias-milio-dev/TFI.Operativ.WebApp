using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.SEC;

namespace Operativ.Web
{
    public partial class CambiarClave : Page
    {
        private readonly IUsuarioBLL _usuarioBLL = FabricaBLL.Instancia.CrearUsuarioBLL();

        protected Panel pnlAviso;
        protected Literal litAviso;
        protected Label lblClaveActual;
        protected TextBox txtClaveActual;
        protected RequiredFieldValidator rfvClaveActual;
        protected Label lblClaveNueva;
        protected TextBox txtClaveNueva;
        protected RequiredFieldValidator rfvClaveNueva;
        protected RegularExpressionValidator revClaveNueva;
        protected Label lblConfirmarClave;
        protected TextBox txtConfirmarClave;
        protected CompareValidator cvConfirmarClave;
        protected Button btnConfirmar;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!ContextoSesion.Actual.EstaAutenticado)
            {
                Response.Redirect("~/Login.aspx", endResponse: true);
                return;
            }

            if (!IsPostBack)
            {
                litAviso.Text = (string)GetGlobalResourceObject("Textos", "LoginClaveTemporalAviso");
                lblClaveActual.Text = (string)GetGlobalResourceObject("Textos", "EtiquetaClaveActual");
                lblClaveNueva.Text = (string)GetGlobalResourceObject("Textos", "EtiquetaClaveNueva");
                lblConfirmarClave.Text = (string)GetGlobalResourceObject("Textos", "EtiquetaConfirmarClave");
                btnConfirmar.Text = (string)GetGlobalResourceObject("Textos", "BotonConfirmar");
                rfvClaveActual.ErrorMessage = (string)GetGlobalResourceObject("Textos", "ValidacionCampoObligatorio");
                rfvClaveNueva.ErrorMessage = (string)GetGlobalResourceObject("Textos", "ValidacionCampoObligatorio");
                revClaveNueva.ErrorMessage = (string)GetGlobalResourceObject("Textos", "ValidacionFormatoInvalido");
                cvConfirmarClave.ErrorMessage = (string)GetGlobalResourceObject("Textos", "ValidacionClaveNoCoincide");
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                _usuarioBLL.CambiarClavePropia(txtClaveActual.Text, txtClaveNueva.Text);
                Response.Redirect("~/Default.aspx", endResponse: true);
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                MensajeError mensaje = ManejadorErrores.Resolver(excepcionNegocio.CodigoError);
                litAviso.Text = $"{mensaje.CodigoError} - {mensaje.Texto}";
                pnlAviso.CssClass = "alert alert-danger";
            }
        }
    }
}

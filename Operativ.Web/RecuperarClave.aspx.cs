using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;

namespace Operativ.Web
{
    public partial class RecuperarClave : Page
    {
        private readonly IUsuarioBLL _usuarioBLL = FabricaBLL.Instancia.CrearUsuarioBLL();

        protected Panel pnlMensaje;
        protected Literal litMensaje;
        protected Label lblUsuario;
        protected TextBox txtUsuario;
        protected RequiredFieldValidator rfvUsuario;
        protected Button btnEnviar;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblUsuario.Text = (string)GetGlobalResourceObject("Textos", "EtiquetaUsuario");
                rfvUsuario.ErrorMessage = (string)GetGlobalResourceObject("Textos", "ValidacionCampoObligatorio");
            }
        }

        protected void btnEnviar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                _usuarioBLL.RecuperarClave(txtUsuario.Text.Trim());

                litMensaje.Text = "Si el usuario existe, se envió un correo con instrucciones para restablecer la contraseña.";
                pnlMensaje.CssClass = "alert alert-success";
                pnlMensaje.Visible = true;
            }
            catch (ExcepcionNegocio)
            {
                litMensaje.Text = "Si el usuario existe, se envió un correo con instrucciones para restablecer la contraseña.";
                pnlMensaje.CssClass = "alert alert-info";
                pnlMensaje.Visible = true;
            }
        }
    }
}

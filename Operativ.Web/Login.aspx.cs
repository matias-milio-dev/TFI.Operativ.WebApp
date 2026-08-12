using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;

namespace Operativ.Web
{
    public partial class Login : Page
    {
        private readonly IUsuarioBLL _usuarioBLL = FabricaBLL.Instancia.CrearUsuarioBLL();

        protected Literal litTitulo;
        protected Panel pnlMensaje;
        protected Literal litMensaje;
        protected Label lblUsuario;
        protected TextBox txtUsuario;
        protected RequiredFieldValidator rfvUsuario;
        protected Label lblClave;
        protected TextBox txtClave;
        protected RequiredFieldValidator rfvClave;
        protected Button btnIngresar;
        protected HyperLink lnkRecuperarClave;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                AsignarTextos();
            }
        }

        private void AsignarTextos()
        {
            litTitulo.Text = (string)GetGlobalResourceObject("Textos", "LoginTitulo");
            lblUsuario.Text = (string)GetGlobalResourceObject("Textos", "EtiquetaUsuario");
            lblClave.Text = (string)GetGlobalResourceObject("Textos", "EtiquetaClave");
            btnIngresar.Text = (string)GetGlobalResourceObject("Textos", "BotonIngresar");
            lnkRecuperarClave.Text = (string)GetGlobalResourceObject("Textos", "LoginOlvideClave");
            rfvUsuario.ErrorMessage = (string)GetGlobalResourceObject("Textos", "ValidacionCampoObligatorio");
            rfvClave.ErrorMessage = (string)GetGlobalResourceObject("Textos", "ValidacionCampoObligatorio");
        }

        protected void btnIngresar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                Usuario usuario = _usuarioBLL.IniciarSesion(txtUsuario.Text.Trim(), txtClave.Text, Request.UserHostAddress);

                if (usuario.ClaveTemporal)
                {
                    Response.Redirect("~/CambiarClave.aspx", endResponse: true);
                    return;
                }

                Response.Redirect("~/Default.aspx", endResponse: true);
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }

        private void MostrarMensaje(string codigoError)
        {
            MensajeError mensaje = ManejadorErrores.Resolver(codigoError);
            litMensaje.Text = $"{mensaje.CodigoError} - {mensaje.Texto}";
            pnlMensaje.Visible = true;
        }
    }
}

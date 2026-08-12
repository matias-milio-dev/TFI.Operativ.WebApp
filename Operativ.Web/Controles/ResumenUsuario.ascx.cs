using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.SEC;

namespace Operativ.Web.Controles
{
    public partial class ResumenUsuario : UserControl
    {
        private readonly IUsuarioBLL _usuarioBLL = FabricaBLL.Instancia.CrearUsuarioBLL();

        protected Panel pnlAutenticado;
        protected Literal litNombreYRol;
        protected HyperLink lnkPerfil;
        protected HyperLink lnkSuscripciones;
        protected LinkButton btnCerrarSesion;
        protected HyperLink lnkIngresar;

        protected void Page_Load(object sender, EventArgs e)
        {
            bool autenticado = ContextoSesion.Actual.EstaAutenticado;
            pnlAutenticado.Visible = autenticado;
            lnkIngresar.Visible = !autenticado;

            if (!autenticado)
            {
                lnkIngresar.Text = (string)GetGlobalResourceObject("Textos", "BotonIngresar");
                return;
            }

            var usuario = ContextoSesion.Actual.UsuarioActual;
            litNombreYRol.Text = $"{usuario.NombreCompleto} ({usuario.CodigoPerfil})";
            lnkPerfil.Text = (string)GetGlobalResourceObject("Textos", "MenuPerfil");
            lnkSuscripciones.Text = (string)GetGlobalResourceObject("Textos", "MenuSuscripciones");
            lnkSuscripciones.Visible = usuario.CodigoPerfil == "CLIENTE";
            btnCerrarSesion.Text = (string)GetGlobalResourceObject("Textos", "MenuSalir");
        }

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            _usuarioBLL.CerrarSesion();
            Response.Redirect("~/Login.aspx", endResponse: true);
        }
    }
}

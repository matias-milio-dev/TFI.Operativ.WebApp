using System;
using System.Web.UI;
using Operativ.BE.Entidades;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;
using Operativ.SEC.Handlers;

namespace Operativ.Web.Controles
{
    public partial class ResumenUsuario : UserControl
    {
        private SesionHandler sesionHandler;

        protected void Page_Load(object sender, EventArgs e)
        {
            sesionHandler = new SesionHandler();

            Usuario usuario = sesionHandler.GetUsuario();
            Familia perfil = sesionHandler.GetPerfil();

            if (usuario == null
                || perfil == null)
            {
                Visible = false;
                return;
            }

            string formatoBienvenida = (string)GetGlobalResourceObject("Textos", "MensajeBienvenida");
            lblBienvenida.Text = string.Format(formatoBienvenida, usuario.NombreUsuario, perfil.Nombre);
        }

        protected void lnkCerrarSesion_Click(object sender, EventArgs e)
        {
            Usuario usuario = sesionHandler.GetUsuario();

            if (usuario != null)
            {
                try
                {
                    FabricaNegocio fabricaNegocio = new FabricaNegocio();
                    IUsuarioNegocio usuarioNegocio = fabricaNegocio.CrearUsuarioNegocio();
                    usuarioNegocio.RegistrarCierreSesion(usuario.IdUsuario);
                }
                catch (Exception)
                {
                }
            }

            sesionHandler.CerrarSesion();
            Response.Redirect("~/Login.aspx");
        }
    }
}

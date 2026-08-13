using System;
using System.Web.UI;
using Operativ.BE.Entidades;
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

            lblBienvenida.Text = "Bienvenido " + usuario.NombreUsuario + ", está logueado como " + perfil.Nombre;
        }

        protected void lnkCerrarSesion_Click(object sender, EventArgs e)
        {
            sesionHandler.CerrarSesion();
            Response.Redirect("~/Login.aspx");
        }
    }
}

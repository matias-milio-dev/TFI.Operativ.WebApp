using System;
using System.Web.UI;
using Operativ.BE.Entidades;
using Operativ.BE.Errores;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.Web.Master;

namespace Operativ.Web.Controles
{
    public partial class ModalCambiarClave : UserControl
    {
        private readonly ErroresHandler erroresHandler = new ErroresHandler();

        protected void btnGuardarClave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            SesionHandler sesionHandler = new SesionHandler();
            Usuario usuario = sesionHandler.GetUsuario();

            if (usuario == null)
            {
                Response.Redirect("~/Login.aspx?err=sesion");
                return;
            }

            try
            {
                FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
                IUsuarioService usuarioService = fabricaSeguridad.CrearUsuarioService();
                usuarioService.CambiarClave(usuario.IdUsuario, txtContrasenaActual.Text, txtContrasenaNueva.Text);

                string mensajeExito = (string)GetGlobalResourceObject("Textos", "MensajeExitoCambioClave");
                ((Principal)Page.Master).ControlNotificaciones.MostrarMensaje(mensajeExito, true);
            }
            catch (Exception excepcion)
            {
                OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
                ((Principal)Page.Master).ControlNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
            }
            finally
            {
                txtContrasenaActual.Text = string.Empty;
                txtContrasenaNueva.Text = string.Empty;
                txtContrasenaConfirmar.Text = string.Empty;
            }
        }
    }
}

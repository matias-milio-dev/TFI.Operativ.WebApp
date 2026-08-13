using System;
using System.Web.UI;
using Operativ.BLL.Contratos;
using Operativ.BLL.Errores;
using Operativ.BLL.Fabricas;

namespace Operativ.Web
{
    public partial class RecuperarContrasena : Page
    {
        private ErroresHandler erroresHandler;

        protected void Page_Load(object sender, EventArgs e)
        {
            erroresHandler = new ErroresHandler();
        }

        protected void btnEnviar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                FabricaNegocio fabricaNegocio = new FabricaNegocio();
                IUsuarioNegocio usuarioNegocio = fabricaNegocio.CrearUsuarioNegocio();
                usuarioNegocio.RecuperarContrasena(txtNombreUsuario.Text.Trim());

                ucNotificaciones.MostrarMensaje("Se envió un email con la contraseña temporal a la casilla registrada.", true);
            }
            catch (Exception excepcion)
            {
                OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
                ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
            }
        }
    }
}

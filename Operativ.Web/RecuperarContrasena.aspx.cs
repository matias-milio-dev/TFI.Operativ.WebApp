using System;
using Operativ.BLL.Contratos;
using Operativ.BLL.Errores;
using Operativ.BLL.Fabricas;
using Operativ.Web.Paginas;

namespace Operativ.Web
{
    public partial class RecuperarContrasena : PaginaBase
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

                string mensajeExito = (string)GetGlobalResourceObject("Textos", "MensajeExitoRecuperacionContrasena");
                ucNotificaciones.MostrarMensaje(mensajeExito, true);
            }
            catch (Exception excepcion)
            {
                OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
                ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
            }
        }
    }
}

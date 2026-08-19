using System;
using Operativ.BE.Errores;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Paginas;

namespace Operativ.Web;
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
            FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
            IUsuarioService usuarioService = fabricaSeguridad.CrearUsuarioService();
            usuarioService.RecuperarContrasena(txtNombreUsuario.Text.Trim());

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

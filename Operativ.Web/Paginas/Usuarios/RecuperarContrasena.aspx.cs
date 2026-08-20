using System;
using Operativ.BE.Errores;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Paginas;

namespace Operativ.Web;
public partial class RecuperarContrasena : PaginaBase
{
    private readonly IUsuarioService usuarioService;

    private readonly ErroresHandler erroresHandler;

    public RecuperarContrasena()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
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

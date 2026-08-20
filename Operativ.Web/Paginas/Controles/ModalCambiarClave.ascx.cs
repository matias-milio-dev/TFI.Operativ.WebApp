using System;
using System.Web.UI;
using Operativ.BE.Entidades;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.Web.Master;

namespace Operativ.Web.Controles;
public partial class ModalCambiarClave : UserControl
{
    private readonly IUsuarioService usuarioService;

    private Notificaciones ControlNotificaciones
    {
        get { return ((Principal)Page.Master).ControlNotificaciones; }
    }

    public ModalCambiarClave()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
    }

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
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?err=sesion");
            return;
        }

        try
        {
            usuarioService.CambiarClave(usuario.IdUsuario, txtContrasenaActual.Text, txtContrasenaNueva.Text);
            ControlNotificaciones.MostrarExito("MensajeExitoCambioClave");
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
        finally
        {
            txtContrasenaActual.Text = string.Empty;
            txtContrasenaNueva.Text = string.Empty;
            txtContrasenaConfirmar.Text = string.Empty;
        }
    }
}

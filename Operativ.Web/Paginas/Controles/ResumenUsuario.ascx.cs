using System;
using System.Web.UI;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;

namespace Operativ.Web.Controles;
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
        lblBienvenida.Text = string.Format(formatoBienvenida, usuario.NombreUsuario, "<strong>" + perfil.Nombre + "</strong>");
    }

    protected void lnkCerrarSesion_Click(object sender, EventArgs e)
    {
        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario != null)
        {
            try
            {
                FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
                IBitacoraService bitacoraService = fabricaSeguridad.CrearBitacoraService();
                bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.CierreSesion);
            }
            catch (Exception)
            {
            }
        }

        sesionHandler.CerrarSesion();
        Response.Redirect("~/Paginas/Usuarios/Login.aspx");
    }
}

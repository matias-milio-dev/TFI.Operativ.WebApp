using System;
using System.Web.UI;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.Web.Idioma;

namespace Operativ.Web.Controles;
public partial class ResumenUsuario : UserControl
{
    private readonly IBitacoraService bitacoraService;
    private readonly SesionHandler sesionHandler;

    public ResumenUsuario()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
        sesionHandler = new SesionHandler();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario == null)
        {
            Visible = false;
            return;
        }

        Familia perfil = sesionHandler.GetPerfil();
        string nombrePerfil = perfil != null
            ? "<strong>" + perfil.Nombre + "</strong>"
            : TextoRecurso.Obtener("EtiquetaSinFamilia");

        lblBienvenida.Text = TextoRecurso.Formato("MensajeBienvenida", usuario.NombreUsuario, nombrePerfil);
    }

    protected void lnkCerrarSesion_Click(object sender, EventArgs e)
    {
        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario != null)
        {
            try
            {
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

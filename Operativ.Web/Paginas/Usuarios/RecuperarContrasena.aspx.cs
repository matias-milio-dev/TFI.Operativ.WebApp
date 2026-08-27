using System;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Paginas;

namespace Operativ.Web;
public partial class RecuperarContrasena : PaginaBase
{
    private readonly IUsuarioService usuarioService;

    public RecuperarContrasena()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            txtNombreUsuario.Text = Request.QueryString["usuario"];
        }
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
            ucNotificaciones.MostrarExito("MensajeExitoRecuperacionContrasena");
        }
        catch (Exception excepcion)
        {
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }
}

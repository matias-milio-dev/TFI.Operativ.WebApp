using System.Text.RegularExpressions;
using System.Web.UI;

namespace Operativ.Web.Controles;
public partial class Notificaciones : UserControl
{
    private static readonly Regex PrefijoCodigoError = new Regex(@"^ERR\d+\s*-\s*");

    public void MostrarMensaje(string mensaje)
    {
        MostrarMensaje(mensaje, false);
    }

    public void MostrarMensaje(string mensaje, bool esExito)
    {
        pnlNotificacion.Visible = true;
        pnlNotificacion.CssClass = esExito ? "notificacion notificacion-exito" : "notificacion notificacion-error";
        lblMensaje.Text = PrefijoCodigoError.Replace(mensaje, string.Empty);
    }
}

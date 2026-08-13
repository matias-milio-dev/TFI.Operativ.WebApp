using System.Web.UI;

namespace Operativ.Web.Controles
{
    public partial class Notificaciones : UserControl
    {
        public void MostrarMensaje(string mensaje)
        {
            MostrarMensaje(mensaje, false);
        }

        public void MostrarMensaje(string mensaje, bool esExito)
        {
            pnlNotificacion.Visible = true;
            pnlNotificacion.CssClass = esExito ? "notificacion notificacion-exito" : "notificacion notificacion-error";
            lblMensaje.Text = mensaje;
        }
    }
}

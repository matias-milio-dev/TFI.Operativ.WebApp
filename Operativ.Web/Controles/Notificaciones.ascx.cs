using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.Comun;

namespace Operativ.Web.Controles
{
    public partial class Notificaciones : UserControl
    {
        protected Panel pnlMensaje;
        protected Literal litMensaje;

        public void MostrarMensaje(string codigoError)
        {
            MensajeError mensaje = ManejadorErrores.Resolver(codigoError);
            MostrarTexto($"{mensaje.CodigoError} - {mensaje.Texto}", mensaje.Tipo);
        }

        public void MostrarExito(string texto)
        {
            pnlMensaje.CssClass = "alert alert-success";
            litMensaje.Text = texto;
            pnlMensaje.Visible = true;
        }

        private void MostrarTexto(string texto, TipoCriticidadError tipo)
        {
            string claseCss;
            switch (tipo)
            {
                case TipoCriticidadError.Advertencia:
                    claseCss = "alert alert-warning";
                    break;
                case TipoCriticidadError.Grave:
                    claseCss = "alert alert-danger";
                    break;
                default:
                    claseCss = "alert alert-danger fw-bold";
                    break;
            }

            pnlMensaje.CssClass = claseCss;
            litMensaje.Text = texto;
            pnlMensaje.Visible = true;
        }
    }
}

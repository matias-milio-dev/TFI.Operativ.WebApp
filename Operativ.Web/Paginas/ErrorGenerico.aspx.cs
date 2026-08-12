using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.Comun;

namespace Operativ.Web.Paginas
{
    public partial class ErrorGenerico : Page
    {
        protected Literal litMensaje;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Operativ_UltimoMensajeError"] is MensajeError mensaje)
            {
                litMensaje.Text = $"{mensaje.CodigoError} - {mensaje.Texto}";
                Session.Remove("Operativ_UltimoMensajeError");
            }
        }
    }
}

using System;
using System.Web.UI;
using Operativ.Web.Controles;

namespace Operativ.Web.Master
{
    public partial class SiteMaster : MasterPage
    {
        protected Notificaciones ucNotificaciones;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public void MostrarMensaje(string codigoError)
        {
            ucNotificaciones.MostrarMensaje(codigoError);
        }

        public void MostrarExito(string texto)
        {
            ucNotificaciones.MostrarExito(texto);
        }
    }
}

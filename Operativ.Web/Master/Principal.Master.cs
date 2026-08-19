using System;
using System.Web.UI;
using Operativ.Web.Controles;

namespace Operativ.Web.Master;
public partial class Principal : MasterPage
{
    public Notificaciones ControlNotificaciones
    {
        get { return ucNotificaciones; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }
}

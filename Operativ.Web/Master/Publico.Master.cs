using System.Web.UI;
using Operativ.Web.Controles;

namespace Operativ.Web.Master;
public partial class Publico : MasterPage
{
    public Notificaciones ControlNotificaciones
    {
        get { return ucNotificaciones; }
    }
}

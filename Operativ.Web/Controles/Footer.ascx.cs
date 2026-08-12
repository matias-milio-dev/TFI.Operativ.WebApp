using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Operativ.Web.Controles
{
    public partial class Footer : UserControl
    {
        protected Literal litDerechos;

        protected void Page_Load(object sender, EventArgs e)
        {
            litDerechos.Text = (string)GetGlobalResourceObject("Textos", "PieDerechosReservados");
        }
    }
}

using System;
using System.Web.UI;
using Operativ.Web.Idioma;

namespace Operativ.Web.Controles
{
    public partial class SelectorIdioma : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string idiomaActual = IdiomaHelper.ObtenerIdiomaActual();

            if (idiomaActual == IdiomaHelper.CodigoIngles)
            {
                lnkIngles.CssClass = "selector-idioma-opcion selector-idioma-activo";
            }
            else
            {
                lnkEspanol.CssClass = "selector-idioma-opcion selector-idioma-activo";
            }
        }

        protected void lnkEspanol_Click(object sender, EventArgs e)
        {
            IdiomaHelper.EstablecerIdioma(IdiomaHelper.CodigoEspanol);
            Response.Redirect(Request.RawUrl);
        }

        protected void lnkIngles_Click(object sender, EventArgs e)
        {
            IdiomaHelper.EstablecerIdioma(IdiomaHelper.CodigoIngles);
            Response.Redirect(Request.RawUrl);
        }
    }
}

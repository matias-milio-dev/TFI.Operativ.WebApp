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
            bool esIngles = idiomaActual == IdiomaHelper.CodigoIngles;

            lnkEspanol.CssClass = esIngles ? "selector-idioma-pill-opcion" : "selector-idioma-pill-opcion activo";
            lnkIngles.CssClass = esIngles ? "selector-idioma-pill-opcion activo" : "selector-idioma-pill-opcion";
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

using System.Globalization;
using System.Threading;
using System.Web.UI;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas
{
    public abstract class PaginaBase : Page
    {
        protected override void InitializeCulture()
        {
            CultureInfo cultura = IdiomaHelper.ObtenerCulturaActual();
            Thread.CurrentThread.CurrentCulture = cultura;
            Thread.CurrentThread.CurrentUICulture = cultura;

            base.InitializeCulture();
        }
    }
}

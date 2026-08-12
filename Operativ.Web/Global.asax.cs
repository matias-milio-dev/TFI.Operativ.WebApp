using System;
using System.Globalization;
using System.Threading;
using System.Web;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;

namespace Operativ.Web
{
    public class Global : HttpApplication
    {
        public const string CookieIdioma = "Operativ_Idioma";
        private static readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        protected void Application_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string idioma = "es";

            HttpCookie cookie = Request.Cookies[CookieIdioma];
            if (cookie != null
                && !string.IsNullOrEmpty(cookie.Value))
            {
                idioma = cookie.Value;
            }

            try
            {
                var cultura = new CultureInfo(idioma);
                Thread.CurrentThread.CurrentCulture = cultura;
                Thread.CurrentThread.CurrentUICulture = cultura;
            }
            catch (CultureNotFoundException)
            {
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception excepcion = Server.GetLastError();
            if (excepcion == null) return;

            MensajeError mensaje = ManejadorErrores.ResolverExcepcion(excepcion);

            try
            {
                _bitacoraBLL.Registrar("ERROR_NO_CONTROLADO", "Sistema", null,
                    excepcion.ToString(), "CRITICA", Request?.UserHostAddress);
            }
            catch
            {
            }

            Session["Operativ_UltimoMensajeError"] = mensaje;
            Server.ClearError();
            Response.Redirect("~/Paginas/ErrorGenerico.aspx", endResponse: true);
        }
    }
}

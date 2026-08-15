using System;
using System.Globalization;
using System.Web;

namespace Operativ.Web.Idioma
{
    public static class IdiomaHelper
    {
        public const string CodigoEspanol = "es";

        public const string CodigoIngles = "en";

        private const string ClaveSession = "Operativ_Idioma";

        private const string ClaveCookie = "Operativ_Idioma";

        public static void EstablecerIdioma(string codigoIdioma)
        {
            HttpContext.Current.Session[ClaveSession] = codigoIdioma;

            HttpCookie cookie = new HttpCookie(ClaveCookie, codigoIdioma);
            cookie.Expires = DateTime.Now.AddYears(1);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static string ObtenerIdiomaActual()
        {
            object idiomaSession = HttpContext.Current.Session[ClaveSession];

            if (idiomaSession != null)
            {
                return idiomaSession.ToString();
            }

            HttpCookie cookie = HttpContext.Current.Request.Cookies[ClaveCookie];

            if (cookie != null
                && !string.IsNullOrEmpty(cookie.Value))
            {
                return cookie.Value;
            }

            return CodigoEspanol;
        }

        public static CultureInfo ObtenerCulturaActual()
        {
            string codigoIdioma = ObtenerIdiomaActual();

            if (codigoIdioma == CodigoIngles)
            {
                return new CultureInfo("en-US");
            }

            return new CultureInfo("es-AR");
        }
    }
}

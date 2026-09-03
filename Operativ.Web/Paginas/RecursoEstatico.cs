using System.IO;
using System.Web;

namespace Operativ.Web.Paginas;
public static class RecursoEstatico
{
    public static string ObtenerUrl(string rutaRelativa)
    {
        string url = VirtualPathUtility.ToAbsolute(rutaRelativa);
        string rutaFisica = HttpContext.Current.Server.MapPath(rutaRelativa);

        if (!File.Exists(rutaFisica))
        {
            return url;
        }

        return url + "?v=" + File.GetLastWriteTimeUtc(rutaFisica).Ticks;
    }
}

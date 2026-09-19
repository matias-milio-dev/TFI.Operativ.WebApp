using System.IO;
using System.Web;

namespace Operativ.WebServices.Xml;
public static class RutaXmlHelper
{
    private const string CarpetaRelativa = "~/App_Data/XmlGenerado";

    public static string ObtenerRutaCompleta(string nombreArchivo)
    {
        string carpeta = HttpContext.Current.Server.MapPath(CarpetaRelativa);

        if (!Directory.Exists(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        return Path.Combine(carpeta, nombreArchivo);
    }
}

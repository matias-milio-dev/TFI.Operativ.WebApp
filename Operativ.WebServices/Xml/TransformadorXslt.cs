using System.IO;
using System.Web;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Operativ.WebServices.Xml;
public static class TransformadorXslt
{
    private const string CarpetaHojasEstilo = "~/Xslt/";

    public static string Transformar(string rutaXml, string nombreHojaEstilo)
    {
        string rutaXslt = HttpContext.Current.Server.MapPath(CarpetaHojasEstilo + nombreHojaEstilo);

        XslCompiledTransform transformacion = new XslCompiledTransform();
        transformacion.Load(rutaXslt);

        XPathDocument documento = new XPathDocument(rutaXml);

        using (StringWriter escritor = new StringWriter())
        {
            transformacion.Transform(documento, null, escritor);
            return escritor.ToString();
        }
    }
}

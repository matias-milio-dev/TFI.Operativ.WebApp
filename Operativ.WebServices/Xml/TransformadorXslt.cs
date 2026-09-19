using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Operativ.WebServices.Xml;
public static class TransformadorXslt
{
    private const string RutaHojaEstilo = "~/Xslt/ResumenSuscripcion.xslt";

    public static string Transformar(string rutaXml)
    {
        string rutaXslt = HttpContext.Current.Server.MapPath(RutaHojaEstilo);

        XslCompiledTransform transformacion = new XslCompiledTransform();
        transformacion.Load(rutaXslt);

        XPathDocument documento = new XPathDocument(rutaXml);
        StringBuilder resultado = new StringBuilder();

        XmlWriterSettings configuracion = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        };

        using (XmlWriter escritor = XmlWriter.Create(resultado, configuracion))
        {
            transformacion.Transform(documento, escritor);
        }

        return resultado.ToString();
    }
}

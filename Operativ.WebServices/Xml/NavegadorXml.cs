using System.Xml;
using System.Xml.XPath;

namespace Operativ.WebServices.Xml;
public static class NavegadorXml
{
    public static XPathNavigator Crear(string rutaCompleta)
    {
        XmlTextReader lector = new XmlTextReader(rutaCompleta);

        try
        {
            XPathDocument documento = new XPathDocument(lector);
            return documento.CreateNavigator();
        }
        finally
        {
            lector.Close();
        }
    }

    public static string LeerValor(XPathNavigator navegador, string expresionXPath)
    {
        XPathNavigator nodo = navegador.SelectSingleNode(expresionXPath);

        if (nodo == null)
        {
            return string.Empty;
        }

        return nodo.Value;
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Operativ.WebServices.Modelos;

namespace Operativ.WebServices.Xml;
public static class LectorResumenSuscripcion
{
    public static ResumenSuscripcionXml Leer(string rutaCompleta)
    {
        XmlTextReader lector = new XmlTextReader(rutaCompleta);
        XPathNavigator navegador;

        try
        {
            XPathDocument documento = new XPathDocument(lector);
            navegador = documento.CreateNavigator();
        }
        finally
        {
            lector.Close();
        }

        ResumenSuscripcionXml resumen = new ResumenSuscripcionXml
        {
            IdSuscripcion = Convert.ToInt32(LeerValor(navegador, "/ResumenSuscripcion/@id")),
            FechaGeneracion = DateTime.Parse(LeerValor(navegador, "/ResumenSuscripcion/@generado"), CultureInfo.InvariantCulture),
            RazonSocial = LeerValor(navegador, "/ResumenSuscripcion/Cliente/RazonSocial"),
            Cuit = LeerValor(navegador, "/ResumenSuscripcion/Cliente/Cuit"),
            EmailContacto = LeerValor(navegador, "/ResumenSuscripcion/Cliente/Email"),
            NombrePlan = LeerValor(navegador, "/ResumenSuscripcion/Plan/Nombre"),
            DescripcionPlan = LeerValor(navegador, "/ResumenSuscripcion/Plan/Descripcion"),
            PrecioAnual = decimal.Parse(LeerValor(navegador, "/ResumenSuscripcion/Plan/PrecioAnual"), CultureInfo.InvariantCulture),
            Estado = LeerValor(navegador, "/ResumenSuscripcion/Condiciones/Estado"),
            FechaAlta = DateTime.Parse(LeerValor(navegador, "/ResumenSuscripcion/Condiciones/FechaAlta"), CultureInfo.InvariantCulture),
            FechaFinTrial = DateTime.Parse(LeerValor(navegador, "/ResumenSuscripcion/Condiciones/FechaFinTrial"), CultureInfo.InvariantCulture),
            DiasTrialRestantes = Convert.ToInt32(LeerValor(navegador, "/ResumenSuscripcion/Condiciones/DiasTrialRestantes")),
            NombreArchivoXml = Path.GetFileName(rutaCompleta)
        };

        return resumen;
    }

    private static string LeerValor(XPathNavigator navegador, string expresionXPath)
    {
        XPathNavigator nodo = navegador.SelectSingleNode(expresionXPath);

        if (nodo == null)
        {
            return string.Empty;
        }

        return nodo.Value;
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Xml.XPath;
using Operativ.WebServices.Modelos;

namespace Operativ.WebServices.Xml;
public static class LectorResumenSuscripcion
{
    public static ResumenSuscripcionXml Leer(string rutaCompleta)
    {
        XPathNavigator navegador = NavegadorXml.Crear(rutaCompleta);

        ResumenSuscripcionXml resumen = new ResumenSuscripcionXml
        {
            IdSuscripcion = Convert.ToInt32(NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/@id")),
            FechaGeneracion = DateTime.Parse(NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/@generado"), CultureInfo.InvariantCulture),
            RazonSocial = NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Cliente/RazonSocial"),
            Cuit = NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Cliente/Cuit"),
            EmailContacto = NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Cliente/Email"),
            NombrePlan = NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Plan/Nombre"),
            DescripcionPlan = NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Plan/Descripcion"),
            PrecioAnual = decimal.Parse(NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Plan/PrecioAnual"), CultureInfo.InvariantCulture),
            Estado = NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Condiciones/Estado"),
            FechaAlta = DateTime.Parse(NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Condiciones/FechaAlta"), CultureInfo.InvariantCulture),
            FechaFinTrial = DateTime.Parse(NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Condiciones/FechaFinTrial"), CultureInfo.InvariantCulture),
            DiasTrialRestantes = Convert.ToInt32(NavegadorXml.LeerValor(navegador, "/ResumenSuscripcion/Condiciones/DiasTrialRestantes")),
            NombreArchivoXml = Path.GetFileName(rutaCompleta)
        };

        return resumen;
    }
}

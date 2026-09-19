using System;
using System.Globalization;
using System.IO;
using System.Xml.XPath;
using Operativ.WebServices.Modelos;

namespace Operativ.WebServices.Xml;
public static class LectorComprobantePago
{
    public static ComprobantePagoXml Leer(string rutaCompleta)
    {
        XPathNavigator navegador = NavegadorXml.Crear(rutaCompleta);

        ComprobantePagoXml comprobante = new ComprobantePagoXml
        {
            CodigoComprobante = NavegadorXml.LeerValor(navegador, "/ComprobantePago/@codigo"),
            RazonSocial = NavegadorXml.LeerValor(navegador, "/ComprobantePago/Cliente/RazonSocial"),
            Cuit = NavegadorXml.LeerValor(navegador, "/ComprobantePago/Cliente/Cuit"),
            IdSuscripcion = Convert.ToInt32(NavegadorXml.LeerValor(navegador, "/ComprobantePago/Pago/IdSuscripcion")),
            NombrePlan = NavegadorXml.LeerValor(navegador, "/ComprobantePago/Pago/Plan"),
            Importe = decimal.Parse(NavegadorXml.LeerValor(navegador, "/ComprobantePago/Pago/Importe"), CultureInfo.InvariantCulture),
            Moneda = NavegadorXml.LeerValor(navegador, "/ComprobantePago/Pago/Moneda"),
            MedioPago = NavegadorXml.LeerValor(navegador, "/ComprobantePago/Pago/MedioPago"),
            FechaPago = DateTime.Parse(NavegadorXml.LeerValor(navegador, "/ComprobantePago/Pago/FechaPago"), CultureInfo.InvariantCulture),
            FechaVencimiento = DateTime.Parse(NavegadorXml.LeerValor(navegador, "/ComprobantePago/Pago/FechaVencimiento"), CultureInfo.InvariantCulture),
            NombreArchivoXml = Path.GetFileName(rutaCompleta)
        };

        return comprobante;
    }
}

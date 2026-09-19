using System;
using System.Globalization;
using System.IO;
using System.Xml.XPath;
using Operativ.WebServices.Modelos;

namespace Operativ.WebServices.Xml;
public static class LectorFactura
{
    public static FacturaXml Leer(string rutaCompleta)
    {
        XPathNavigator navegador = NavegadorXml.Crear(rutaCompleta);

        FacturaXml factura = new FacturaXml
        {
            NumeroFactura = NavegadorXml.LeerValor(navegador, "/Factura/@numero"),
            FechaEmision = DateTime.Parse(NavegadorXml.LeerValor(navegador, "/Factura/@emitida"), CultureInfo.InvariantCulture),
            RazonSocial = NavegadorXml.LeerValor(navegador, "/Factura/Receptor/RazonSocial"),
            Cuit = NavegadorXml.LeerValor(navegador, "/Factura/Receptor/Cuit"),
            Email = NavegadorXml.LeerValor(navegador, "/Factura/Receptor/Email"),
            NombrePlan = NavegadorXml.LeerValor(navegador, "/Factura/Detalle/Item/Descripcion"),
            Total = decimal.Parse(NavegadorXml.LeerValor(navegador, "/Factura/Totales/Total"), CultureInfo.InvariantCulture),
            Moneda = NavegadorXml.LeerValor(navegador, "/Factura/Totales/Moneda"),
            CodigoComprobantePago = NavegadorXml.LeerValor(navegador, "/Factura/ComprobantePago"),
            NombreArchivoXml = Path.GetFileName(rutaCompleta)
        };

        return factura;
    }
}

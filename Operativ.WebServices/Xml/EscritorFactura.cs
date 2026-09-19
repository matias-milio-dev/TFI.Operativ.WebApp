using System;
using System.Globalization;
using System.Text;
using System.Xml;
using Operativ.BE.Entidades;

namespace Operativ.WebServices.Xml;
public static class EscritorFactura
{
    public static string Escribir(Factura factura, string nombreArchivo)
    {
        string rutaCompleta = RutaXmlHelper.ObtenerRutaCompleta(nombreArchivo);

        XmlTextWriter escritor = new XmlTextWriter(rutaCompleta, Encoding.UTF8);

        try
        {
            escritor.Formatting = Formatting.Indented;
            escritor.Indentation = 2;

            escritor.WriteStartDocument();
            escritor.WriteStartElement("Factura");
            escritor.WriteAttributeString("numero", factura.NumeroFactura);
            escritor.WriteAttributeString("emitida", factura.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss"));

            escritor.WriteStartElement("Receptor");
            escritor.WriteElementString("RazonSocial", factura.RazonSocialCliente);
            escritor.WriteElementString("Cuit", factura.CuitCliente);
            escritor.WriteElementString("Email", factura.EmailCliente);
            escritor.WriteEndElement();

            escritor.WriteStartElement("Detalle");
            escritor.WriteStartElement("Item");
            escritor.WriteElementString("Descripcion", "Suscripción anual — " + factura.NombrePlan);
            escritor.WriteElementString("Cantidad", "1");
            escritor.WriteElementString("Importe", factura.Total.ToString("F2", CultureInfo.InvariantCulture));
            escritor.WriteEndElement();
            escritor.WriteEndElement();

            escritor.WriteStartElement("Totales");
            escritor.WriteElementString("Total", factura.Total.ToString("F2", CultureInfo.InvariantCulture));
            escritor.WriteElementString("Moneda", factura.Moneda);
            escritor.WriteEndElement();

            escritor.WriteElementString("ComprobantePago", factura.CodigoComprobantePago);

            escritor.WriteEndElement();
            escritor.WriteEndDocument();
            escritor.Flush();
        }
        finally
        {
            escritor.Close();
        }

        return rutaCompleta;
    }
}

using System;
using System.Globalization;
using System.Text;
using System.Xml;
using Operativ.BE.Entidades;

namespace Operativ.WebServices.Xml;
public static class EscritorComprobantePago
{
    public static string Escribir(Suscripcion suscripcion, string moneda, string nombreArchivo)
    {
        string rutaCompleta = RutaXmlHelper.ObtenerRutaCompleta(nombreArchivo);

        XmlTextWriter escritor = new XmlTextWriter(rutaCompleta, Encoding.UTF8);

        try
        {
            escritor.Formatting = Formatting.Indented;
            escritor.Indentation = 2;

            escritor.WriteStartDocument();
            escritor.WriteStartElement("ComprobantePago");
            escritor.WriteAttributeString("codigo", suscripcion.CodigoComprobante);
            escritor.WriteAttributeString("generado", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));

            escritor.WriteStartElement("Cliente");
            escritor.WriteElementString("RazonSocial", suscripcion.RazonSocialCliente);
            escritor.WriteElementString("Cuit", suscripcion.CuitCliente);
            escritor.WriteEndElement();

            escritor.WriteStartElement("Pago");
            escritor.WriteElementString("IdSuscripcion", suscripcion.IdSuscripcion.ToString());
            escritor.WriteElementString("Plan", suscripcion.NombrePlan);
            escritor.WriteElementString("Importe", suscripcion.PrecioAnual.ToString("F2", CultureInfo.InvariantCulture));
            escritor.WriteElementString("Moneda", moneda);
            escritor.WriteElementString("MedioPago", suscripcion.MedioPago.ToString());
            escritor.WriteElementString("FechaPago", suscripcion.FechaPago.Value.ToString("yyyy-MM-dd"));
            escritor.WriteElementString("FechaVencimiento", suscripcion.FechaVencimiento.Value.ToString("yyyy-MM-dd"));
            escritor.WriteEndElement();

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

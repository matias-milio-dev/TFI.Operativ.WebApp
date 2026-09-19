using System;
using System.Globalization;
using System.Text;
using System.Xml;
using Operativ.BE.Entidades;

namespace Operativ.WebServices.Xml;
public static class EscritorResumenSuscripcion
{
    public static string Escribir(Suscripcion suscripcion, string nombreArchivo)
    {
        string rutaCompleta = RutaXmlHelper.ObtenerRutaCompleta(nombreArchivo);

        XmlTextWriter escritor = new XmlTextWriter(rutaCompleta, Encoding.UTF8);

        try
        {
            escritor.Formatting = Formatting.Indented;
            escritor.Indentation = 2;

            escritor.WriteStartDocument();
            escritor.WriteStartElement("ResumenSuscripcion");
            escritor.WriteAttributeString("id", suscripcion.IdSuscripcion.ToString());
            escritor.WriteAttributeString("generado", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));

            escritor.WriteStartElement("Cliente");
            escritor.WriteElementString("RazonSocial", suscripcion.RazonSocialCliente);
            escritor.WriteElementString("Cuit", suscripcion.CuitCliente);
            escritor.WriteElementString("Email", suscripcion.EmailCliente);
            escritor.WriteEndElement();

            escritor.WriteStartElement("Plan");
            escritor.WriteElementString("Nombre", suscripcion.NombrePlan);
            escritor.WriteElementString("Descripcion", suscripcion.DescripcionPlan);
            escritor.WriteElementString("PrecioAnual", suscripcion.PrecioAnual.ToString("F2", CultureInfo.InvariantCulture));
            escritor.WriteEndElement();

            escritor.WriteStartElement("Condiciones");
            escritor.WriteElementString("Estado", suscripcion.Estado.ToString());
            escritor.WriteElementString("FechaAlta", suscripcion.FechaAlta.ToString("yyyy-MM-dd"));
            escritor.WriteElementString("FechaFinTrial", suscripcion.FechaFinTrial.ToString("yyyy-MM-dd"));
            escritor.WriteElementString("DiasTrialRestantes", suscripcion.DiasTrialRestantes.ToString());
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

using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;
using Operativ.Comun;

namespace Operativ.WebServices
{
    internal static class XmlHelper
    {
        private static readonly XslCompiledTransform _transformResumenSuscripcion = CargarTransformacion("Operativ.WebServices.Xslt.ResumenSuscripcion.xslt");

        public static string EscribirIncidente(IncidenteXml incidente, string nombreArchivo)
        {
            return EscribirArchivo(nombreArchivo, escritor =>
            {
                escritor.WriteStartElement("Incidente");
                escritor.WriteElementString("IdIncidente", incidente.IdIncidente.ToString(CultureInfo.InvariantCulture));
                escritor.WriteElementString("IdActivo", incidente.IdActivo.ToString(CultureInfo.InvariantCulture));
                escritor.WriteElementString("Descripcion", incidente.Descripcion);
                escritor.WriteElementString("Prioridad", incidente.Prioridad);
                escritor.WriteElementString("Estado", incidente.Estado);
                escritor.WriteElementString("FechaAlta", FormatoFecha(incidente.FechaAlta));
                escritor.WriteEndElement();
            });
        }

        public static string EscribirCatalogo(CatalogoXml catalogo, string nombreArchivo)
        {
            return EscribirArchivo(nombreArchivo, escritor =>
            {
                escritor.WriteStartElement("Catalogo");
                escritor.WriteElementString("Filtro", catalogo.Filtro ?? string.Empty);
                escritor.WriteElementString("FechaConsulta", FormatoFecha(catalogo.FechaConsulta));
                escritor.WriteStartElement("Paquetes");
                foreach (var paquete in catalogo.Paquetes)
                {
                    escritor.WriteStartElement("Paquete");
                    escritor.WriteElementString("IdPaquete", paquete.IdPaquete.ToString(CultureInfo.InvariantCulture));
                    escritor.WriteElementString("Nombre", paquete.Nombre);
                    escritor.WriteElementString("PrecioBase", paquete.PrecioBase.ToString(CultureInfo.InvariantCulture));
                    escritor.WriteElementString("CantidadActivosIncluidos", paquete.CantidadActivosIncluidos.ToString(CultureInfo.InvariantCulture));
                    escritor.WriteEndElement();
                }
                escritor.WriteEndElement();
                escritor.WriteEndElement();
            });
        }

        public static string EscribirResumenSuscripcion(ResumenSuscripcionXml resumen, string nombreArchivo)
        {
            return EscribirArchivo(nombreArchivo, escritor =>
            {
                escritor.WriteStartElement("ResumenSuscripcion");
                escritor.WriteElementString("Cuit", resumen.Cuit ?? string.Empty);
                escritor.WriteElementString("RazonSocial", resumen.RazonSocial);
                escritor.WriteElementString("CorreoElectronico", resumen.CorreoElectronico);
                escritor.WriteElementString("IdPaquete", resumen.IdPaquete.ToString(CultureInfo.InvariantCulture));
                escritor.WriteElementString("NombrePaquete", resumen.NombrePaquete);
                escritor.WriteElementString("PrecioBase", resumen.PrecioBase.ToString(CultureInfo.InvariantCulture));
                escritor.WriteElementString("FechaGeneracion", FormatoFecha(resumen.FechaGeneracion));
                escritor.WriteEndElement();
            });
        }

        public static string TransformarResumenSuscripcion(string rutaArchivoXml)
        {
            using (var lector = new XmlTextReader(rutaArchivoXml))
            using (var escritorTexto = new StringWriter())
            {
                _transformResumenSuscripcion.Transform(lector, null, escritorTexto);
                return escritorTexto.ToString();
            }
        }

        private static string EscribirArchivo(string nombreArchivo, System.Action<XmlTextWriter> escribirContenido)
        {
            string carpeta = ConfiguracionAplicacion.RutaXmlGenerado;
            Directory.CreateDirectory(carpeta);
            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var escritor = new XmlTextWriter(rutaCompleta, System.Text.Encoding.UTF8))
            {
                escritor.Formatting = Formatting.Indented;
                escritor.WriteStartDocument();
                escribirContenido(escritor);
                escritor.WriteEndDocument();
            }

            return rutaCompleta;
        }

        private static string FormatoFecha(System.DateTime valor) => valor.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        private static XslCompiledTransform CargarTransformacion(string nombreRecursoEmbebido)
        {
            var transformacion = new XslCompiledTransform();
            using (var flujo = Assembly.GetExecutingAssembly().GetManifestResourceStream(nombreRecursoEmbebido))
            using (var lector = XmlReader.Create(flujo))
            {
                transformacion.Load(lector);
            }
            return transformacion;
        }
    }
}

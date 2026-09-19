using System.Web.Services;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.WebServices.Modelos;
using Operativ.WebServices.Xml;

namespace Operativ.WebServices;

[WebService(Namespace = "http://operativ.local/webservices/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class ResumenSuscripcion : WebService
{
    private const string PrefijoArchivo = "resumen_suscripcion_";

    private readonly ISuscripcionRepositorio suscripcionRepositorio;

    public ResumenSuscripcion()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        suscripcionRepositorio = fabricaRepositorio.CrearSuscripcionRepositorio();
    }

    [WebMethod(Description = "Genera el resumen previo a la confirmación de una suscripción y lo persiste como XML.")]
    public ResumenSuscripcionXml GenerarResumen(int idSuscripcion)
    {
        Suscripcion suscripcion = suscripcionRepositorio.GetPorId(idSuscripcion);

        if (suscripcion == null)
        {
            return null;
        }

        string nombreArchivo = PrefijoArchivo + idSuscripcion.ToString() + ".xml";
        string rutaCompleta = EscritorResumenSuscripcion.Escribir(suscripcion, nombreArchivo);

        ResumenSuscripcionXml resumen = LectorResumenSuscripcion.Leer(rutaCompleta);
        resumen.ResumenHtml = TransformadorXslt.Transformar(rutaCompleta);

        return resumen;
    }
}

using System.Web.Services;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.WebServices.Modelos;
using Operativ.WebServices.Xml;

namespace Operativ.WebServices;

[WebService(Namespace = "http://operativ.local/webservices/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class ComprobantePago : WebService
{
    private const string PrefijoArchivo = "comprobante_pago_";
    private const string HojaEstilo = "ComprobantePago.xslt";

    private readonly ISuscripcionRepositorio suscripcionRepositorio;

    public ComprobantePago()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        suscripcionRepositorio = fabricaRepositorio.CrearSuscripcionRepositorio();
    }

    [WebMethod(Description = "Genera el comprobante de pago de una suscripción y lo persiste como XML.")]
    public ComprobantePagoXml GenerarComprobante(int idSuscripcion, string moneda)
    {
        Suscripcion suscripcion = suscripcionRepositorio.GetPorId(idSuscripcion);

        if (suscripcion == null || !suscripcion.FechaPago.HasValue)
        {
            return null;
        }

        string nombreArchivo = PrefijoArchivo + suscripcion.CodigoComprobante + ".xml";
        string rutaCompleta = EscritorComprobantePago.Escribir(suscripcion, moneda, nombreArchivo);

        ComprobantePagoXml comprobante = LectorComprobantePago.Leer(rutaCompleta);
        comprobante.ComprobanteHtml = TransformadorXslt.Transformar(rutaCompleta, HojaEstilo);

        return comprobante;
    }
}

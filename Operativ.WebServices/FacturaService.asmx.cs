using System.Web.Services;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.WebServices.Modelos;
using Operativ.WebServices.Xml;

namespace Operativ.WebServices;

[WebService(Namespace = "http://operativ.local/webservices/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class FacturaService : WebService
{
    private const string PrefijoArchivo = "factura_";
    private const string HojaEstilo = "Factura.xslt";

    private readonly IFacturaRepositorio facturaRepositorio;

    public FacturaService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        facturaRepositorio = fabricaRepositorio.CrearFacturaRepositorio();
    }

    [WebMethod(Description = "Genera el XML de una factura emitida y devuelve su representación.")]
    public FacturaXml GenerarFactura(int idFactura)
    {
        Factura factura = facturaRepositorio.GetPorId(idFactura);

        if (factura == null)
        {
            return null;
        }

        string nombreArchivo = PrefijoArchivo + factura.NumeroFactura + ".xml";
        string rutaCompleta = EscritorFactura.Escribir(factura, nombreArchivo);

        FacturaXml facturaXml = LectorFactura.Leer(rutaCompleta);
        facturaXml.FacturaHtml = TransformadorXslt.Transformar(rutaCompleta, HojaEstilo);

        return facturaXml;
    }
}

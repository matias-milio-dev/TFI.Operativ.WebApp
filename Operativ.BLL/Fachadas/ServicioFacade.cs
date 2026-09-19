using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.WebServices;
using Operativ.WebServices.Modelos;

namespace Operativ.BLL.Fachadas;
public class ServicioFacade
{
    public ResumenSuscripcionXml GenerarResumenSuscripcion(int idSuscripcion)
    {
        ResumenSuscripcion servicio = new ResumenSuscripcion();

        ResumenSuscripcionXml resumen = servicio.GenerarResumen(idSuscripcion);

        if (resumen == null)
        {
            throw new OperativException(TipoError.ErrorGeneracionResumenSuscripcion);
        }

        return resumen;
    }

    public ComprobantePagoXml GenerarComprobantePago(int idSuscripcion, string moneda)
    {
        ComprobantePago servicio = new ComprobantePago();

        ComprobantePagoXml comprobante = servicio.GenerarComprobante(idSuscripcion, moneda);

        if (comprobante == null)
        {
            throw new OperativException(TipoError.ErrorGeneracionComprobantePago);
        }

        return comprobante;
    }

    public FacturaXml GenerarFactura(int idFactura)
    {
        FacturaService servicio = new FacturaService();

        FacturaXml factura = servicio.GenerarFactura(idFactura);

        if (factura == null)
        {
            throw new OperativException(TipoError.ErrorGeneracionFactura);
        }

        return factura;
    }
}

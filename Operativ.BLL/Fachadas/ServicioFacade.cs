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
}

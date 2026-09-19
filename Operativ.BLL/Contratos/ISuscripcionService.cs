using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.WebServices.Modelos;

namespace Operativ.BLL.Contratos;
public interface ISuscripcionService
{
    List<Plan> ListarPlanes();

    List<Suscripcion> ListarSuscripciones(string filtro, int? idCliente, int numeroPagina, int tamanioPagina);

    int ContarSuscripciones(string filtro, int? idCliente);

    Suscripcion ObtenerSuscripcionVigente(int idCliente);

    Suscripcion ObtenerSuscripcionPorId(int idSuscripcion);

    bool TieneSuscripcionActiva(int idCliente);

    ResumenSuscripcionXml GenerarResumen(int idSuscripcion);

    int AltaSuscripcion(int idCliente, int idPlan);

    void CancelarSuscripcion(int idSuscripcion);
}

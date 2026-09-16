using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.BLL.Contratos;
public interface IServicioService
{
    List<Servicio> ListarServicios();
}

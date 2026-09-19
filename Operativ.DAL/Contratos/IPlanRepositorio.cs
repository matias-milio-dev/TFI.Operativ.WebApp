using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IPlanRepositorio
{
    List<Plan> ListarActivos();

    Plan GetPorId(int idPlan);
}

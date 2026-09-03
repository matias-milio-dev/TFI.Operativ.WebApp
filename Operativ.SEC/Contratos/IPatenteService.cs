using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IPatenteService
{
    List<Patente> ListarTodas();

    List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario);

    void AsignarPatentes(int idUsuario, List<int> idsPatente);

    void QuitarPatentes(int idUsuario, List<int> idsPatente);
}

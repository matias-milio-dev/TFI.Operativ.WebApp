using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IPatenteService
{
    List<Patente> ListarTodas();

    List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario);

    void AsignarPatentes(int idUsuario, int[] idsPatente);

    void QuitarPatentes(int idUsuario, int[] idsPatente);
}

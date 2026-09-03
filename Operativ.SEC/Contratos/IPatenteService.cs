using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IPatenteService
{
    List<Patente> ListarTodas();

    List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario);

    void AsignarPatente(int idUsuario, int idPatente);

    void QuitarPatente(int idUsuario, int idPatente);
}

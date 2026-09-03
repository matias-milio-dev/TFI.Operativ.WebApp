using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IPatenteRepositorio
{
    List<Patente> ListarTodas();

    List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario);

    void AsignarPatentesAUsuario(int idUsuario, List<int> idsPatente);

    void QuitarPatentesDeUsuario(int idUsuario, List<int> idsPatente);
}

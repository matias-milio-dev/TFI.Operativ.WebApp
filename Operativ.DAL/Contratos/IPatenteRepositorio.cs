using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IPatenteRepositorio
{
    List<Patente> ListarTodas();

    List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario);

    void AsignarPatenteAUsuario(int idUsuario, int idPatente);

    void QuitarPatenteDeUsuario(int idUsuario, int idPatente);

    bool ExistePatenteIndividual(int idUsuario, int idPatente);
}

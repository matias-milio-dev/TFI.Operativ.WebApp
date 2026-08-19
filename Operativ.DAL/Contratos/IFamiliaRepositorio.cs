using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IFamiliaRepositorio
{
    List<Familia> GetFamiliasDeUsuario(int idUsuario);

    List<Patente> GetPatentesDeFamilia(int idFamilia);

    List<Familia> ListarTodas();
}

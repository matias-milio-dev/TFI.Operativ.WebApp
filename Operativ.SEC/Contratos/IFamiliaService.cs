using System.Collections.Generic;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IFamiliaService
{
    Familia GetPerfilDeUsuario(int idUsuario);

    FamiliaCompuesto ArmarArbolPermisos(int idUsuario);

    List<Familia> ListarFamilias();

    Familia GetFamiliaPorId(int idFamilia);

    List<Patente> GetPatentesDeFamilia(int idFamilia);

    void AsignarPatentes(int idFamilia, int[] idsPatente);

    void QuitarPatentes(int idFamilia, int[] idsPatente);
}

using System.Collections.Generic;
using Operativ.BE.Composite;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IFamiliaService
{
    Familia GetPerfilDeUsuario(int idUsuario);

    FamiliaCompuesto ArmarArbolPermisos(int idUsuario);

    List<Familia> ListarFamilias();
}

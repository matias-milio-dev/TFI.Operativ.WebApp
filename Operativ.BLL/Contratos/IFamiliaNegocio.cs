using Operativ.BE.Composite;
using Operativ.BE.Entidades;

namespace Operativ.BLL.Contratos
{
    public interface IFamiliaNegocio
    {
        Familia GetPerfilDeUsuario(int idUsuario);

        FamiliaCompuesto ArmarArbolPermisos(int idUsuario);
    }
}

using Operativ.BLL.Contratos;
using Operativ.BLL.Implementaciones;

namespace Operativ.BLL.Fabricas
{
    public class FabricaNegocio
    {
        public IUsuarioNegocio CrearUsuarioNegocio()
        {
            return new UsuarioNegocio();
        }

        public IFamiliaNegocio CrearFamiliaNegocio()
        {
            return new FamiliaNegocio();
        }
    }
}

using Operativ.SEC.Contratos;
using Operativ.SEC.Implementaciones;

namespace Operativ.SEC.Fabricas
{
    public class FabricaSeguridad
    {
        public IUsuarioService CrearUsuarioService()
        {
            return new UsuarioService();
        }

        public IFamiliaService CrearFamiliaService()
        {
            return new FamiliaService();
        }

        public IBitacoraService CrearBitacoraService()
        {
            return new BitacoraService();
        }

        public IIntegridadService CrearIntegridadService()
        {
            return new IntegridadService();
        }
    }
}

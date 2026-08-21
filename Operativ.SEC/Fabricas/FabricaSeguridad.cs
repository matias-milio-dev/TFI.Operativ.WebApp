using Operativ.SEC.Contratos;
using Operativ.SEC.Implementaciones;
using Operativ.SEC.Implementaciones.Estrategias;

namespace Operativ.SEC.Fabricas;
public class FabricaSeguridad
{
    public ILoginStrategy CrearLoginStrategy(bool modoEmergencia = false)
    {
        return modoEmergencia ? new LoginEmergenciaStrategy() : new LoginNormalStrategy();
    }

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

using Operativ.BLL.Contratos;
using Operativ.BLL.Implementaciones;

namespace Operativ.BLL.Fabricas;
public class FabricaNegocio
{
    public IServicioService CrearServicioService()
    {
        return new ServicioService();
    }

    public IPaqueteService CrearPaqueteService()
    {
        return new PaqueteService();
    }
}
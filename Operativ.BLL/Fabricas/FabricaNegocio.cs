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

    public IActivoService CrearActivoService()
    {
        return new ActivoService();
    }

    public IClienteService CrearClienteService()
    {
        return new ClienteService();
    }

    public IIncidenteService CrearIncidenteService()
    {
        return new IncidenteService();
    }
}
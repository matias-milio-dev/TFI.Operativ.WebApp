using Operativ.DAL.Contratos;
using Operativ.DAL.Implementaciones;

namespace Operativ.DAL.Fabricas;
public class FabricaRepositorio
{
    public IUsuarioRepositorio CrearUsuarioRepositorio()
    {
        return new UsuarioRepositorio();
    }

    public IFamiliaRepositorio CrearFamiliaRepositorio()
    {
        return new FamiliaRepositorio();
    }

    public IBitacoraRepositorio CrearBitacoraRepositorio()
    {
        return new BitacoraRepositorio();
    }

    public IIntegridadRepositorio CrearIntegridadRepositorio()
    {
        return new IntegridadRepositorio();
    }
}

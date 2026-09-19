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

    public IPatenteRepositorio CrearPatenteRepositorio()
    {
        return new PatenteRepositorio();
    }

    public IBackupRepositorio CrearBackupRepositorio()
    {
        return new BackupRepositorio();
    }

    public IBitacoraRepositorio CrearBitacoraRepositorio()
    {
        return new BitacoraRepositorio();
    }

    public IIntegridadRepositorio CrearIntegridadRepositorio()
    {
        return new IntegridadRepositorio();
    }

    public IPaqueteRepositorio CrearPaqueteRepositorio()
    {
        return new PaqueteRepositorio();
    }

    public IProgramaRepositorio CrearProgramaRepositorio()
    {
        return new ProgramaRepositorio();
    }

    public IActivoRepositorio CrearActivoRepositorio()
    {
        return new ActivoRepositorio();
    }

    public IClienteRepositorio CrearClienteRepositorio()
    {
        return new ClienteRepositorio();
    }
}

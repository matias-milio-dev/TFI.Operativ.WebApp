using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.SEC.Implementaciones;
public class PatenteService : IPatenteService
{
    private readonly IPatenteRepositorio patenteRepositorio;
    private readonly IUsuarioRepositorio usuarioRepositorio;
    private readonly IBitacoraService bitacoraService;

    public PatenteService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        patenteRepositorio = fabricaRepositorio.CrearPatenteRepositorio();
        usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public List<Patente> ListarTodas()
    {
        return patenteRepositorio.ListarTodas();
    }

    public List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario)
    {
        return patenteRepositorio.GetPatentesIndividualesDeUsuario(idUsuario);
    }

    public void AsignarPatente(int idUsuario, int idPatente)
    {
        ValidarUsuarioExistente(idUsuario);

        if (patenteRepositorio.ExistePatenteIndividual(idUsuario, idPatente))
        {
            throw new OperativException(TipoError.ErrorPatenteYaAsignada);
        }

        patenteRepositorio.AsignarPatenteAUsuario(idUsuario, idPatente);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.AsignacionPatente);
    }

    public void QuitarPatente(int idUsuario, int idPatente)
    {
        ValidarUsuarioExistente(idUsuario);

        if (!patenteRepositorio.ExistePatenteIndividual(idUsuario, idPatente))
        {
            throw new OperativException(TipoError.ErrorPatenteNoAsignada);
        }

        patenteRepositorio.QuitarPatenteDeUsuario(idUsuario, idPatente);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.RemocionPatente);
    }

    private void ValidarUsuarioExistente(int idUsuario)
    {
        if (usuarioRepositorio.GetPorId(idUsuario) == null)
        {
            throw new OperativException(TipoError.ErrorUsuarioNoExiste);
        }
    }
}

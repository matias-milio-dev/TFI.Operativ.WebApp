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

    public void AsignarPatentes(int idUsuario, int[] idsPatente)
    {
        if (idsPatente.Length == 0)
        {
            return;
        }

        ValidarUsuarioExistente(idUsuario);

        List<Patente> asignadas = patenteRepositorio.GetPatentesIndividualesDeUsuario(idUsuario);

        foreach (int idPatente in idsPatente)
        {
            if (EstaAsignada(asignadas, idPatente))
            {
                throw new OperativException(TipoError.ErrorPatenteYaAsignada);
            }
        }

        patenteRepositorio.AsignarPatentesAUsuario(idUsuario, idsPatente);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.AsignacionPatente, DescribirPatentes(idsPatente));
    }

    public void QuitarPatentes(int idUsuario, int[] idsPatente)
    {
        if (idsPatente.Length == 0)
        {
            return;
        }

        ValidarUsuarioExistente(idUsuario);

        List<Patente> asignadas = patenteRepositorio.GetPatentesIndividualesDeUsuario(idUsuario);

        foreach (int idPatente in idsPatente)
        {
            if (!EstaAsignada(asignadas, idPatente))
            {
                throw new OperativException(TipoError.ErrorPatenteNoAsignada);
            }
        }

        patenteRepositorio.QuitarPatentesDeUsuario(idUsuario, idsPatente);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.RemocionPatente, DescribirPatentes(idsPatente));
    }

    private string DescribirPatentes(int[] idsPatente)
    {
        List<string> nombres = new List<string>();

        foreach (Patente patente in patenteRepositorio.ListarTodas())
        {
            if (ContieneId(idsPatente, patente.IdPatente))
            {
                nombres.Add(patente.Nombre);
            }
        }

        return string.Join(", ", nombres);
    }

    private bool EstaAsignada(List<Patente> patentes, int idPatente)
    {
        foreach (Patente patente in patentes)
        {
            if (patente.IdPatente == idPatente)
            {
                return true;
            }
        }

        return false;
    }

    private bool ContieneId(int[] ids, int id)
    {
        foreach (int idActual in ids)
        {
            if (idActual == id)
            {
                return true;
            }
        }

        return false;
    }

    private void ValidarUsuarioExistente(int idUsuario)
    {
        if (usuarioRepositorio.GetPorId(idUsuario) == null)
        {
            throw new OperativException(TipoError.ErrorUsuarioNoExiste);
        }
    }
}

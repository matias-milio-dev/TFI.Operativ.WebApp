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

    public void AsignarPatentes(int idUsuario, List<int> idsPatente)
    {
        if (idsPatente.Count == 0)
        {
            return;
        }

        ValidarUsuarioExistente(idUsuario);

        List<int> idsAsignadas = ObtenerIdsPatentesIndividuales(idUsuario);

        foreach (int idPatente in idsPatente)
        {
            if (idsAsignadas.Contains(idPatente))
            {
                throw new OperativException(TipoError.ErrorPatenteYaAsignada);
            }
        }

        patenteRepositorio.AsignarPatentesAUsuario(idUsuario, idsPatente);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.AsignacionPatente, DescribirPatentes(idsPatente));
    }

    public void QuitarPatentes(int idUsuario, List<int> idsPatente)
    {
        if (idsPatente.Count == 0)
        {
            return;
        }

        ValidarUsuarioExistente(idUsuario);

        List<int> idsAsignadas = ObtenerIdsPatentesIndividuales(idUsuario);

        foreach (int idPatente in idsPatente)
        {
            if (!idsAsignadas.Contains(idPatente))
            {
                throw new OperativException(TipoError.ErrorPatenteNoAsignada);
            }
        }

        patenteRepositorio.QuitarPatentesDeUsuario(idUsuario, idsPatente);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.RemocionPatente, DescribirPatentes(idsPatente));
    }

    private string DescribirPatentes(List<int> idsPatente)
    {
        List<string> nombres = new List<string>();

        foreach (Patente patente in patenteRepositorio.ListarTodas())
        {
            if (idsPatente.Contains(patente.IdPatente))
            {
                nombres.Add(patente.Nombre);
            }
        }

        return string.Join(", ", nombres);
    }

    private List<int> ObtenerIdsPatentesIndividuales(int idUsuario)
    {
        List<int> ids = new List<int>();

        foreach (Patente patente in patenteRepositorio.GetPatentesIndividualesDeUsuario(idUsuario))
        {
            ids.Add(patente.IdPatente);
        }

        return ids;
    }

    private void ValidarUsuarioExistente(int idUsuario)
    {
        if (usuarioRepositorio.GetPorId(idUsuario) == null)
        {
            throw new OperativException(TipoError.ErrorUsuarioNoExiste);
        }
    }
}

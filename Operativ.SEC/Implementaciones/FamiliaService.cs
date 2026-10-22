using System;
using System.Collections.Generic;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;

namespace Operativ.SEC.Implementaciones;
public class FamiliaService : IFamiliaService
{
    private readonly IFamiliaRepositorio familiaRepositorio;
    private readonly IPatenteRepositorio patenteRepositorio;
    private readonly IBitacoraService bitacoraService;

    public FamiliaService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        familiaRepositorio = fabricaRepositorio.CrearFamiliaRepositorio();
        patenteRepositorio = fabricaRepositorio.CrearPatenteRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public Familia GetPerfilDeUsuario(int idUsuario)
    {
        List<Familia> familias = familiaRepositorio.GetFamiliasDeUsuario(idUsuario);

        if (familias.Count == 0)
        {
            return null;
        }

        return familias[0];
    }

    public FamiliaCompuesto ArmarArbolPermisos(int idUsuario)
    {
        FamiliaCompuesto raiz = new FamiliaCompuesto
        {
            Id = idUsuario,
            Nombre = "PermisosDeUsuario"
        };

        Familia perfil = GetPerfilDeUsuario(idUsuario);

        if (perfil != null)
        {
            raiz.Agregar(ArmarRamaFamilia(perfil));
        }

        List<Patente> patentesIndividuales = patenteRepositorio.GetPatentesIndividualesDeUsuario(idUsuario);

        foreach (Patente patente in patentesIndividuales)
        {
            UsuarioPatenteHoja hoja = new UsuarioPatenteHoja
            {
                Id = patente.IdPatente,
                Nombre = patente.Nombre
            };
            raiz.Agregar(hoja);
        }

        return raiz;
    }

    public List<Familia> ListarFamilias()
    {
        return familiaRepositorio.ListarTodas();
    }

    public List<Patente> GetPatentesDeFamilia(int idFamilia)
    {
        return familiaRepositorio.GetPatentesDeFamilia(idFamilia);
    }

    public Familia GetFamiliaPorId(int idFamilia)
    {
        Familia familia = familiaRepositorio.GetPorId(idFamilia)
            ?? throw new OperativException(TipoError.ErrorFamiliaNoExiste);

        return familia;
    }

    public void AsignarPatentes(int idFamilia, int[] idsPatente)
    {
        if (idsPatente.Length == 0)
        {
            return;
        }

        Familia familia = GetFamiliaPorId(idFamilia);
        List<Patente> asignadas = familiaRepositorio.GetPatentesDeFamilia(idFamilia);

        foreach (int idPatente in idsPatente)
        {
            if (ContieneIdPatente(asignadas, idPatente))
            {
                throw new OperativException(TipoError.ErrorPatenteYaAsignada);
            }
        }

        familiaRepositorio.AsignarPatentesAFamilia(idFamilia, idsPatente);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.AsignacionPatenteFamilia, familia.Nombre + ": " + DescribirPatentes(idsPatente));
    }

    public void QuitarPatentes(int idFamilia, int[] idsPatente)
    {
        if (idsPatente.Length == 0)
        {
            return;
        }

        Familia familia = GetFamiliaPorId(idFamilia);
        List<Patente> asignadas = familiaRepositorio.GetPatentesDeFamilia(idFamilia);

        foreach (int idPatente in idsPatente)
        {
            if (!ContieneIdPatente(asignadas, idPatente))
            {
                throw new OperativException(TipoError.ErrorPatenteNoAsignada);
            }
        }

        familiaRepositorio.QuitarPatentesDeFamilia(idFamilia, idsPatente);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.RemocionPatenteFamilia, familia.Nombre + ": " + DescribirPatentes(idsPatente));
    }

    private string DescribirPatentes(int[] idsPatente)
    {
        List<string> nombres = new List<string>();

        foreach (Patente patente in patenteRepositorio.ListarTodas())
        {
            if (Array.IndexOf(idsPatente, patente.IdPatente) >= 0)
            {
                nombres.Add(patente.Nombre);
            }
        }

        return string.Join(", ", nombres);
    }

    private bool ContieneIdPatente(List<Patente> patentes, int idPatente)
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

    private int? ObtenerIdUsuarioActual()
    {
        SesionHandler sesionHandler = new SesionHandler();
        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario == null || usuario.IdUsuario <= 0)
        {
            return null;
        }

        return usuario.IdUsuario;
    }

    private FamiliaCompuesto ArmarRamaFamilia(Familia perfil)
    {
        FamiliaCompuesto ramaFamilia = new FamiliaCompuesto
        {
            Id = perfil.IdFamilia,
            Nombre = perfil.Nombre
        };

        List<Patente> patentesFamilia = familiaRepositorio.GetPatentesDeFamilia(perfil.IdFamilia);

        foreach (Patente patente in patentesFamilia)
        {
            UsuarioPatenteHoja hoja = new UsuarioPatenteHoja
            {
                Id = patente.IdPatente,
                Nombre = patente.Nombre
            };
            ramaFamilia.Agregar(hoja);
        }

        return ramaFamilia;
    }
}

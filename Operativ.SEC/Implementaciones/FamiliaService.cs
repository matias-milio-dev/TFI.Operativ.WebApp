using System.Collections.Generic;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones;
public class FamiliaService : IFamiliaService
{
    private readonly IFamiliaRepositorio familiaRepositorio;
    private readonly IPatenteRepositorio patenteRepositorio;

    public FamiliaService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        familiaRepositorio = fabricaRepositorio.CrearFamiliaRepositorio();
        patenteRepositorio = fabricaRepositorio.CrearPatenteRepositorio();
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

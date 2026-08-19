using System.Collections.Generic;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones;
public class FamiliaService : IFamiliaService
{
    private readonly IFamiliaRepositorio familiaRepositorio;

    public FamiliaService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        familiaRepositorio = fabricaRepositorio.CrearFamiliaRepositorio();
    }

    public Familia GetPerfilDeUsuario(int idUsuario)
    {
        List<Familia> familias = familiaRepositorio.GetFamiliasDeUsuario(idUsuario);

        if (familias.Count == 0)
        {
            throw new OperativException(TipoError.ErrorUsuarioNoExiste);
        }

        return familias[0];
    }

    public FamiliaCompuesto ArmarArbolPermisos(int idUsuario)
    {
        Familia perfil = GetPerfilDeUsuario(idUsuario);

        FamiliaCompuesto raiz = new FamiliaCompuesto
        {
            Id = perfil.IdFamilia,
            Nombre = perfil.Nombre
        };

        List<Patente> patentes = familiaRepositorio.GetPatentesDeFamilia(perfil.IdFamilia);

        foreach (Patente patente in patentes)
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
}

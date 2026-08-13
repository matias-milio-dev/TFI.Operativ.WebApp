using System.Collections.Generic;
using Operativ.BE.Composite;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BLL.Contratos;
using Operativ.BLL.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;

namespace Operativ.BLL.Implementaciones
{
    public class FamiliaNegocio : IFamiliaNegocio
    {
        private readonly IFamiliaRepositorio familiaRepositorio;

        public FamiliaNegocio()
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
    }
}

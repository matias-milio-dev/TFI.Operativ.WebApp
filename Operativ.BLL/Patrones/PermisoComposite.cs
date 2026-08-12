using System.Collections.Generic;
using System.Linq;
using Operativ.BE;

namespace Operativ.BLL.Patrones
{
    public interface IComponentePermiso
    {
        string Nombre { get; }
        IEnumerable<Patente> ObtenerPatentes();
    }

    public class PatenteComponente : IComponentePermiso
    {
        private readonly Patente _patente;

        public PatenteComponente(Patente patente)
        {
            _patente = patente;
        }

        public string Nombre => _patente.Nombre;

        public IEnumerable<Patente> ObtenerPatentes()
        {
            yield return _patente;
        }
    }

    public class FamiliaComponente : IComponentePermiso
    {
        private readonly Familia _familia;
        private readonly List<IComponentePermiso> _hijos = new List<IComponentePermiso>();

        public FamiliaComponente(Familia familia)
        {
            _familia = familia;
            foreach (var patente in familia.Patentes)
            {
                _hijos.Add(new PatenteComponente(patente));
            }
        }

        public string Nombre => _familia.Nombre;

        public void Agregar(IComponentePermiso componente) => _hijos.Add(componente);

        public IEnumerable<Patente> ObtenerPatentes()
        {
            return _hijos.SelectMany(hijo => hijo.ObtenerPatentes()).Distinct();
        }
    }

    public static class ArmadorPermisosEfectivos
    {
        public static List<Patente> Armar(List<Familia> familias, List<Patente> patentesDirectas)
        {
            var raiz = new List<IComponentePermiso>();
            foreach (var familia in familias)
            {
                raiz.Add(new FamiliaComponente(familia));
            }
            foreach (var patente in patentesDirectas)
            {
                raiz.Add(new PatenteComponente(patente));
            }

            return raiz.SelectMany(componente => componente.ObtenerPatentes())
                       .GroupBy(p => p.IdPatente)
                       .Select(grupo => grupo.First())
                       .ToList();
        }
    }
}

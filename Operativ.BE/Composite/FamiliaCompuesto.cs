using System.Collections.Generic;

namespace Operativ.BE.Composite
{
    public class FamiliaCompuesto : ComponentePermiso
    {
        private readonly List<ComponentePermiso> componentes;

        public FamiliaCompuesto()
        {
            componentes = new List<ComponentePermiso>();
        }

        public void Agregar(ComponentePermiso componente)
        {
            componentes.Add(componente);
        }

        public void Quitar(ComponentePermiso componente)
        {
            componentes.Remove(componente);
        }

        public List<ComponentePermiso> ObtenerComponentes()
        {
            return componentes;
        }

        public override List<string> ObtenerNombresPatentes()
        {
            List<string> nombres = new List<string>();

            foreach (ComponentePermiso componente in componentes)
            {
                nombres.AddRange(componente.ObtenerNombresPatentes());
            }

            return nombres;
        }
    }
}

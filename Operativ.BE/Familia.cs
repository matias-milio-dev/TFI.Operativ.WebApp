using System.Collections.Generic;

namespace Operativ.BE
{
    public class Familia
    {
        public int IdFamilia { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }

        public List<Patente> Patentes { get; set; }

        public Familia()
        {
            Patentes = new List<Patente>();
        }
    }
}

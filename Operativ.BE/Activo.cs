using System;

namespace Operativ.BE
{
    public class Activo
    {
        public int IdActivo { get; set; }
        public int IdCliente { get; set; }
        public int? IdSuscripcion { get; set; }
        public string Nombre { get; set; }
        public string TipoActivo { get; set; }
        public string Identificador { get; set; }
        public bool EstaActivo { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}

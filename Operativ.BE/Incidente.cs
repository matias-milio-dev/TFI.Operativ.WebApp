using System;

namespace Operativ.BE
{
    public class Incidente
    {
        public int IdIncidente { get; set; }
        public int IdActivo { get; set; }
        public string CodigoCategoria { get; set; }
        public string Descripcion { get; set; }
        public string Prioridad { get; set; }
        public string Estado { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string RutaXmlGenerado { get; set; }
    }
}

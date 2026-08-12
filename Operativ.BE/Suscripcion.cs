using System;

namespace Operativ.BE
{
    public class Suscripcion
    {
        public int IdSuscripcion { get; set; }
        public int IdCliente { get; set; }
        public int IdPaquete { get; set; }
        public string NombrePaquete { get; set; }
        public string CodigoEstado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal PrecioAcordado { get; set; }
        public string EstrategiaAplicada { get; set; }
    }
}

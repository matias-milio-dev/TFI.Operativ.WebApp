using System;

namespace Operativ.BE
{
    public class Pago
    {
        public int IdPago { get; set; }
        public int IdSuscripcion { get; set; }
        public string CodigoMedioPago { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string ReferenciaExterna { get; set; }
    }
}

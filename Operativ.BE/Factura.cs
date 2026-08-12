using System;

namespace Operativ.BE
{
    public class Factura
    {
        public int IdFactura { get; set; }
        public int IdPago { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
    }
}

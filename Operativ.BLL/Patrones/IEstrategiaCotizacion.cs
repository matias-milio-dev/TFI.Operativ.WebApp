using Operativ.BE;

namespace Operativ.BLL.Patrones
{
    public interface IEstrategiaCotizacion
    {
        string Nombre { get; }
        decimal Calcular(Paquete paquete, int mesesDuracion);
    }

    public class EstrategiaCotizacionEstandar : IEstrategiaCotizacion
    {
        public string Nombre => "Estandar";

        public decimal Calcular(Paquete paquete, int mesesDuracion)
        {
            return paquete.PrecioBase * mesesDuracion;
        }
    }

    public class EstrategiaCotizacionAnualConDescuento : IEstrategiaCotizacion
    {
        private const decimal PorcentajeDescuento = 0.10m;

        public string Nombre => "AnualConDescuento";

        public decimal Calcular(Paquete paquete, int mesesDuracion)
        {
            decimal precioBruto = paquete.PrecioBase * mesesDuracion;
            return precioBruto - (precioBruto * PorcentajeDescuento);
        }
    }

    public static class SelectorEstrategiaCotizacion
    {
        public static IEstrategiaCotizacion Seleccionar(int mesesDuracion)
        {
            return mesesDuracion >= 12
                ? (IEstrategiaCotizacion)new EstrategiaCotizacionAnualConDescuento()
                : new EstrategiaCotizacionEstandar();
        }
    }
}

using Operativ.Comun;

namespace Operativ.BLL.Patrones
{
    public interface IProcesadorMedioPago
    {
        string Codigo { get; }
        void Validar(decimal monto, string referenciaExterna);
    }

    public class ProcesadorMedioPagoTarjeta : IProcesadorMedioPago
    {
        public string Codigo => "TARJETA";

        public void Validar(decimal monto, string referenciaExterna)
        {
            if (string.IsNullOrWhiteSpace(referenciaExterna))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }
        }
    }

    public class ProcesadorMedioPagoTransferencia : IProcesadorMedioPago
    {
        public string Codigo => "TRANSFERENCIA";

        public void Validar(decimal monto, string referenciaExterna)
        {
            if (string.IsNullOrWhiteSpace(referenciaExterna))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }
        }
    }

    public class ProcesadorMedioPagoEfectivo : IProcesadorMedioPago
    {
        public string Codigo => "EFECTIVO";

        public void Validar(decimal monto, string referenciaExterna)
        {
        }
    }

    public static class FabricaProcesadorMedioPago
    {
        public static IProcesadorMedioPago Obtener(string codigoMedioPago)
        {
            switch (codigoMedioPago)
            {
                case "TARJETA": return new ProcesadorMedioPagoTarjeta();
                case "TRANSFERENCIA": return new ProcesadorMedioPagoTransferencia();
                case "EFECTIVO": return new ProcesadorMedioPagoEfectivo();
                default: throw new ExcepcionNegocio(CodigosError.ErrorMedioPagoNoSoportado);
            }
        }
    }
}

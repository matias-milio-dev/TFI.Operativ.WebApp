using System.Data;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.DAL;

namespace Operativ.BLL
{
    public interface IFacturaBLL
    {
        DataTable ListarPorCliente(int idCliente);
        int Emitir(int idPago, decimal montoTotal);
    }

    public class FacturaBLL : IFacturaBLL
    {
        private readonly IFacturaDAL _facturaDAL = FabricaDAL.Instancia.CrearFacturaDAL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public DataTable ListarPorCliente(int idCliente)
        {
            return _facturaDAL.ListarPorCliente(idCliente);
        }

        public int Emitir(int idPago, decimal montoTotal)
        {
            if (montoTotal <= 0)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }

            string numeroFactura = $"0001-{idPago:00000000}";
            int idFacturaNueva = _facturaDAL.Insertar(idPago, numeroFactura, montoTotal);

            _bitacoraBLL.Registrar("ALTA", "Factura", idFacturaNueva.ToString(), $"Emisión de factura {numeroFactura}.", "ADVERTENCIA");
            return idFacturaNueva;
        }
    }
}

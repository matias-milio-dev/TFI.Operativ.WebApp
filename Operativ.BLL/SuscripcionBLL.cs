using System;
using System.Data;
using Operativ.BE;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.DAL;

namespace Operativ.BLL
{
    public interface ISuscripcionBLL
    {
        Operativ.WebServices.ResumenSuscripcionXml GenerarResumen(string cuit, string razonSocial, string correoElectronico, int idPaquete);
        DataTable ListarPorCliente(int idCliente);
        int Alta(int idCliente, int idPaquete, int mesesDuracion);
        void Cancelar(int idSuscripcion);
        int RegistrarPago(int idSuscripcion, string codigoMedioPago, decimal monto, string referenciaExterna);
    }

    public class SuscripcionBLL : ISuscripcionBLL
    {
        private readonly ServicioFacade _servicioFacade = new ServicioFacade();
        private readonly ISuscripcionDAL _suscripcionDAL = FabricaDAL.Instancia.CrearSuscripcionDAL();
        private readonly IPagoDAL _pagoDAL = FabricaDAL.Instancia.CrearPagoDAL();
        private readonly IPaqueteBLL _paqueteBLL = FabricaBLL.Instancia.CrearPaqueteBLL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public Operativ.WebServices.ResumenSuscripcionXml GenerarResumen(string cuit, string razonSocial, string correoElectronico, int idPaquete)
        {
            return _servicioFacade.GenerarResumenSuscripcion(cuit, razonSocial, correoElectronico, idPaquete);
        }

        public DataTable ListarPorCliente(int idCliente)
        {
            return _suscripcionDAL.ListarPorCliente(idCliente);
        }

        public int Alta(int idCliente, int idPaquete, int mesesDuracion)
        {
            if (mesesDuracion <= 0)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }

            Paquete paquete = _paqueteBLL.Obtener(idPaquete);
            if (!paquete.Activo)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorPaqueteInactivo);
            }

            IEstrategiaCotizacion estrategia = SelectorEstrategiaCotizacion.Seleccionar(mesesDuracion);
            decimal precioAcordado = estrategia.Calcular(paquete, mesesDuracion);

            DateTime fechaInicio = DateTime.Today;
            DateTime fechaVencimiento = fechaInicio.AddMonths(mesesDuracion);

            int idSuscripcionNueva = _suscripcionDAL.Insertar(idCliente, idPaquete, fechaInicio, fechaVencimiento, precioAcordado, estrategia.Nombre);

            _bitacoraBLL.Registrar("ALTA", "Suscripcion", idSuscripcionNueva.ToString(),
                $"Alta de suscripción al paquete '{paquete.Nombre}' por {mesesDuracion} mes(es) usando estrategia '{estrategia.Nombre}'.", "ADVERTENCIA");

            return idSuscripcionNueva;
        }

        public void Cancelar(int idSuscripcion)
        {
            _suscripcionDAL.CambiarEstado(idSuscripcion, "CANCELADA");
            _bitacoraBLL.Registrar("BAJA", "Suscripcion", idSuscripcion.ToString(), "Cancelación de suscripción.", "GRAVE");
        }

        public int RegistrarPago(int idSuscripcion, string codigoMedioPago, decimal monto, string referenciaExterna)
        {
            IProcesadorMedioPago procesador = FabricaProcesadorMedioPago.Obtener(codigoMedioPago);
            procesador.Validar(monto, referenciaExterna);

            int idPagoNuevo = _pagoDAL.Insertar(idSuscripcion, codigoMedioPago, monto, referenciaExterna);

            _suscripcionDAL.CambiarEstado(idSuscripcion, "ACTIVA");

            _bitacoraBLL.Registrar("PAGO", "Pago", idPagoNuevo.ToString(),
                $"Pago registrado para suscripción {idSuscripcion} vía {codigoMedioPago}.", "ADVERTENCIA");

            return idPagoNuevo;
        }
    }
}

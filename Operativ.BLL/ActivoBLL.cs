using System.Data;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.DAL;

namespace Operativ.BLL
{
    public interface IActivoBLL
    {
        DataTable ListarPorCliente(int idCliente);
        int Alta(int idCliente, int idSuscripcion, string nombre, string tipoActivo, string identificador);
        void Baja(int idActivo);
    }

    public class ActivoBLL : IActivoBLL
    {
        private readonly IActivoDAL _activoDAL = FabricaDAL.Instancia.CrearActivoDAL();
        private readonly ISuscripcionDAL _suscripcionDAL = FabricaDAL.Instancia.CrearSuscripcionDAL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public DataTable ListarPorCliente(int idCliente)
        {
            return _activoDAL.ListarPorCliente(idCliente);
        }

        public int Alta(int idCliente, int idSuscripcion, string nombre, string tipoActivo, string identificador)
        {
            if (string.IsNullOrWhiteSpace(nombre)
                || string.IsNullOrWhiteSpace(tipoActivo))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }

            var suscripcion = _suscripcionDAL.ObtenerPorId(idSuscripcion);
            if (suscripcion == null
                || (string)suscripcion["CodigoEstado"] != "ACTIVA")
            {
                throw new ExcepcionNegocio(CodigosError.ErrorActivoSinSuscripcionActiva);
            }

            int idActivoNuevo = _activoDAL.Insertar(idCliente, idSuscripcion, nombre, tipoActivo, identificador);
            _bitacoraBLL.Registrar("ALTA", "Activo", idActivoNuevo.ToString(), $"Alta de activo '{nombre}'.", "ADVERTENCIA");
            return idActivoNuevo;
        }

        public void Baja(int idActivo)
        {
            _activoDAL.Baja(idActivo);
            _bitacoraBLL.Registrar("BAJA", "Activo", idActivo.ToString(), "Baja lógica de activo.", "GRAVE");
        }
    }
}

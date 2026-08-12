using System.Data;
using Operativ.BE;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.DAL;

namespace Operativ.BLL
{
    public interface IPaqueteBLL
    {
        DataTable Listar(bool soloActivos = true);
        Paquete Obtener(int idPaquete);
        int Alta(Paquete paquete);
        void Modificar(Paquete paquete);
        void Baja(int idPaquete);
    }

    public class PaqueteBLL : IPaqueteBLL
    {
        private readonly IPaqueteDAL _paqueteDAL = FabricaDAL.Instancia.CrearPaqueteDAL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public DataTable Listar(bool soloActivos = true)
        {
            return _paqueteDAL.Listar(soloActivos);
        }

        public Paquete Obtener(int idPaquete)
        {
            var paquete = _paqueteDAL.ObtenerPorId(idPaquete);
            if (paquete == null) throw new ExcepcionNegocio(CodigosError.ErrorRegistroNoEncontrado);
            return paquete;
        }

        public int Alta(Paquete paquete)
        {
            ValidarPaquete(paquete);

            int idPaqueteNuevo = _paqueteDAL.Insertar(paquete);
            _bitacoraBLL.Registrar("ALTA", "Paquete", idPaqueteNuevo.ToString(), $"Alta de paquete '{paquete.Nombre}'.", "ADVERTENCIA");
            return idPaqueteNuevo;
        }

        public void Modificar(Paquete paquete)
        {
            ValidarPaquete(paquete);

            _paqueteDAL.Modificar(paquete);
            _bitacoraBLL.Registrar("MODIFICACION", "Paquete", paquete.IdPaquete.ToString(), "Modificación de paquete.", "ADVERTENCIA");
        }

        public void Baja(int idPaquete)
        {
            _paqueteDAL.Baja(idPaquete);
            _bitacoraBLL.Registrar("BAJA", "Paquete", idPaquete.ToString(), "Baja lógica de paquete.", "GRAVE");
        }

        private static void ValidarPaquete(Paquete paquete)
        {
            if (string.IsNullOrWhiteSpace(paquete.Nombre)
                || paquete.PrecioBase <= 0)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }
        }
    }
}

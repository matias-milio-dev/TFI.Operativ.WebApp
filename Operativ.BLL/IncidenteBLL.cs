using System.Data;
using Operativ.BLL.Patrones;
using Operativ.DAL;
using Operativ.WebServices;

namespace Operativ.BLL
{
    public interface IIncidenteBLL
    {
        DataTable ListarPorActivo(int idActivo);
        IncidenteXml Alta(int idActivo, string descripcion, string prioridad, string codigoCategoria);
        void Cerrar(int idIncidente);
    }

    public class IncidenteBLL : IIncidenteBLL
    {
        private readonly ServicioFacade _facade = new ServicioFacade();
        private readonly IIncidenteDAL _incidenteDAL = FabricaDAL.Instancia.CrearIncidenteDAL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public DataTable ListarPorActivo(int idActivo)
        {
            return _incidenteDAL.ListarPorActivo(idActivo);
        }

        public IncidenteXml Alta(int idActivo, string descripcion, string prioridad, string codigoCategoria)
        {
            return _facade.GenerarIncidente(idActivo, descripcion, prioridad, codigoCategoria);
        }

        public void Cerrar(int idIncidente)
        {
            _incidenteDAL.Cerrar(idIncidente);
            _bitacoraBLL.Registrar("MODIFICACION", "Incidente", idIncidente.ToString(), "Cierre de incidente.", "ADVERTENCIA");
        }
    }
}

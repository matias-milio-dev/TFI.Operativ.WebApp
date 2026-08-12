using Operativ.WebServices;

namespace Operativ.BLL.Patrones
{
    public class ServicioFacade
    {
        private readonly IncidentesService _incidentesService = new IncidentesService();
        private readonly CatalogoService _catalogoService = new CatalogoService();
        private readonly ResumenSuscripcion _resumenSuscripcionService = new ResumenSuscripcion();

        public IncidenteXml GenerarIncidente(int idActivo, string descripcion, string prioridad, string codigoCategoria)
        {
            return _incidentesService.GenerarIncidente(idActivo, descripcion, prioridad, codigoCategoria);
        }

        public CatalogoXml ConsultarCatalogo(string filtro)
        {
            return _catalogoService.ConsultarCatalogo(filtro);
        }

        public ResumenSuscripcionXml GenerarResumenSuscripcion(string cuit, string razonSocial, string correoElectronico, int idPaquete)
        {
            return _resumenSuscripcionService.GenerarResumen(cuit, razonSocial, correoElectronico, idPaquete);
        }
    }
}

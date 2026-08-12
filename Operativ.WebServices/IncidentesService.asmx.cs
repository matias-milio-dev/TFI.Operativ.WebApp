using System;
using System.Web.Services;
using Operativ.Comun;
using Operativ.DAL;

namespace Operativ.WebServices
{
    [WebService(Namespace = "http://operativ.local/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class IncidentesService : WebService
    {
        private readonly IIncidenteDAL _incidenteDAL = FabricaDAL.Instancia.CrearIncidenteDAL();
        private readonly IBitacoraDAL _bitacoraDAL = FabricaDAL.Instancia.CrearBitacoraDAL();

        [WebMethod]
        public IncidenteXml GenerarIncidente(int idActivo, string descripcion, string prioridad, string codigoCategoria)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }

            int idIncidenteNuevo = _incidenteDAL.Insertar(idActivo, codigoCategoria, descripcion, prioridad);

            var incidenteXml = new IncidenteXml
            {
                IdIncidente = idIncidenteNuevo,
                IdActivo = idActivo,
                Descripcion = descripcion,
                Prioridad = prioridad,
                Estado = "ABIERTO",
                FechaAlta = DateTime.Now
            };

            string rutaArchivo = XmlHelper.EscribirIncidente(incidenteXml, $"incidente_{idIncidenteNuevo}.xml");
            _incidenteDAL.ActualizarRutaXml(idIncidenteNuevo, rutaArchivo);

            _bitacoraDAL.Registrar(null, "ALTA", "Incidente", idIncidenteNuevo.ToString(),
                "Incidente generado vía IncidentesService", "ADVERTENCIA", null);

            return incidenteXml;
        }
    }
}

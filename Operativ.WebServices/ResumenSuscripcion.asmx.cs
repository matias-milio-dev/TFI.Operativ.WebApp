using System;
using System.Web.Services;
using Operativ.Comun;
using Operativ.DAL;

namespace Operativ.WebServices
{
    [WebService(Namespace = "http://operativ.local/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class ResumenSuscripcion : WebService
    {
        private readonly IPaqueteDAL _paqueteDAL = FabricaDAL.Instancia.CrearPaqueteDAL();

        [WebMethod]
        public ResumenSuscripcionXml GenerarResumen(string cuit, string razonSocial, string correoElectronico, int idPaquete)
        {
            var paquete = _paqueteDAL.ObtenerPorId(idPaquete);
            if (paquete == null)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorPaqueteInactivo);
            }

            var resumen = new ResumenSuscripcionXml
            {
                Cuit = cuit,
                RazonSocial = razonSocial,
                CorreoElectronico = correoElectronico,
                IdPaquete = paquete.IdPaquete,
                NombrePaquete = paquete.Nombre,
                PrecioBase = paquete.PrecioBase,
                FechaGeneracion = DateTime.Now
            };

            string identificador = string.IsNullOrEmpty(cuit) ? Guid.NewGuid().ToString("N") : cuit.Replace("-", "");
            string rutaArchivo = XmlHelper.EscribirResumenSuscripcion(resumen, $"resumen_suscripcion_{identificador}.xml");
            resumen.ResumenHtml = XmlHelper.TransformarResumenSuscripcion(rutaArchivo);

            return resumen;
        }
    }
}

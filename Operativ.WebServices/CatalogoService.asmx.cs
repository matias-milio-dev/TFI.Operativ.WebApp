using System;
using System.Data;
using System.Web.Services;
using Operativ.DAL;

namespace Operativ.WebServices
{
    [WebService(Namespace = "http://operativ.local/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class CatalogoService : WebService
    {
        private readonly IPaqueteDAL _paqueteDAL = FabricaDAL.Instancia.CrearPaqueteDAL();

        [WebMethod]
        public CatalogoXml ConsultarCatalogo(string filtro)
        {
            DataTable tabla = _paqueteDAL.Listar(soloActivos: true);

            var catalogo = new CatalogoXml
            {
                Filtro = filtro,
                FechaConsulta = DateTime.Now
            };

            foreach (DataRow fila in tabla.Rows)
            {
                string nombre = (string)fila["Nombre"];
                if (!string.IsNullOrEmpty(filtro)
                    && nombre.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                catalogo.Paquetes.Add(new PaqueteXml
                {
                    IdPaquete = (int)fila["IdPaquete"],
                    Nombre = nombre,
                    PrecioBase = (decimal)fila["PrecioBase"],
                    CantidadActivosIncluidos = (int)fila["CantidadActivosIncluidos"]
                });
            }

            XmlHelper.EscribirCatalogo(catalogo, "catalogo_paquetes.xml");

            return catalogo;
        }
    }
}

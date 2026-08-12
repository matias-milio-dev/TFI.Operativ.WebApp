using Operativ.BLL.Patrones;
using Operativ.WebServices;

namespace Operativ.BLL
{
    public interface ICatalogoBLL
    {
        CatalogoXml Consultar(string filtro);
    }

    public class CatalogoBLL : ICatalogoBLL
    {
        private readonly ServicioFacade _facade = new ServicioFacade();

        public CatalogoXml Consultar(string filtro)
        {
            return _facade.ConsultarCatalogo(filtro);
        }
    }
}

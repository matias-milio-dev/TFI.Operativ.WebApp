using System.Data;
using Operativ.DAL;

namespace Operativ.BLL
{
    public interface IPagoBLL
    {
        DataTable ListarPorSuscripcion(int idSuscripcion);
    }

    public class PagoBLL : IPagoBLL
    {
        private readonly IPagoDAL _pagoDAL = FabricaDAL.Instancia.CrearPagoDAL();

        public DataTable ListarPorSuscripcion(int idSuscripcion)
        {
            return _pagoDAL.ListarPorSuscripcion(idSuscripcion);
        }
    }
}

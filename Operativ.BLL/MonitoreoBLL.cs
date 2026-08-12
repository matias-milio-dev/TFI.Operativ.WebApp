using System.Data;
using Operativ.DAL;

namespace Operativ.BLL
{
    public interface IMonitoreoBLL
    {
        DataRow ObtenerIndicadores(int? idCliente);
    }

    public class MonitoreoBLL : IMonitoreoBLL
    {
        private readonly ISistemaDAL _sistemaDAL = FabricaDAL.Instancia.CrearSistemaDAL();

        public DataRow ObtenerIndicadores(int? idCliente)
        {
            return _sistemaDAL.ObtenerIndicadoresMonitoreo(idCliente);
        }
    }
}

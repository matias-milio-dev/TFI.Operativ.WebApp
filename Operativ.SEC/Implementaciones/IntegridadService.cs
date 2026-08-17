using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones
{
    public class IntegridadService : IIntegridadService
    {
        private readonly IIntegridadRepositorio integridadRepositorio;

        public IntegridadService()
        {
            FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
            integridadRepositorio = fabricaRepositorio.CrearIntegridadRepositorio();
        }

        public void InicializarDigitos()
        {
            if (!integridadRepositorio.ExisteLineaBase())
            {
                integridadRepositorio.RecalcularTodo();
            }
        }
    }
}

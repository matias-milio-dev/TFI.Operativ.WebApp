namespace Operativ.DAL.Contratos
{
    public interface IIntegridadRepositorio
    {
        bool ExisteLineaBase();

        void RecalcularTodo();
    }
}

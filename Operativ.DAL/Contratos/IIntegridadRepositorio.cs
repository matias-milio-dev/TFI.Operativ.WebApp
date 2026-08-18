using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos
{
    public interface IIntegridadRepositorio
    {
        bool ExisteLineaBase();

        void RecalcularTodo();

        List<ResultadoVerificacionTabla> VerificarTodo();
    }
}

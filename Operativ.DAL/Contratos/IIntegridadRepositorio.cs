using System.Collections.Generic;
using Operativ.BE.Modelos;

namespace Operativ.DAL.Contratos;
public interface IIntegridadRepositorio
{
    bool ExisteTablaDigitosVerticiales();

    void RecalcularTodo();

    List<ResultadoVerificacionTabla> VerificarTodo();
}

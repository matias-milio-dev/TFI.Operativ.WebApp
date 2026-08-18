using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos
{
    public interface IIntegridadService
    {
        void InicializarDigitos();

        List<ResultadoVerificacionTabla> VerificarIntegridad();

        string FormatearResumenFallas(List<ResultadoVerificacionTabla> resultados);

        void RepararBaseDatos();
    }
}

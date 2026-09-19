using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.BLL.Contratos;
public interface IIncidenteService
{
    List<Incidente> ListarIncidentes(string filtro, EstadoIncidente? estado, int? idCliente, int numeroPagina, int tamanioPagina);

    int ContarIncidentes(string filtro, EstadoIncidente? estado, int? idCliente);

    Incidente ObtenerIncidentePorId(int idIncidente);

    int AltaIncidente(Incidente incidente, int idCliente);

    void CerrarIncidente(int idIncidente, string comentarioResolucion);
}

using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.DAL.Contratos;
public interface IIncidenteRepositorio
{
    List<Incidente> Listar(string filtro, EstadoIncidente? estado, int? idCliente, int numeroPagina, int tamanioPagina);

    int ContarIncidentes(string filtro, EstadoIncidente? estado, int? idCliente);

    Incidente GetPorId(int idIncidente);

    int ContarIncidentesDelAnio(int anio);

    int Insertar(Incidente incidente);

    void Cerrar(int idIncidente, string comentarioResolucion);
}

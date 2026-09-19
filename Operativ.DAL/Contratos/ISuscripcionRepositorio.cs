using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface ISuscripcionRepositorio
{
    List<Suscripcion> Listar(string filtro, int? idCliente, int numeroPagina, int tamanioPagina);

    int ContarSuscripciones(string filtro, int? idCliente);

    Suscripcion GetPorId(int idSuscripcion);

    Suscripcion GetVigentePorCliente(int idCliente);

    int Insertar(Suscripcion suscripcion);

    void Cancelar(int idSuscripcion);
}

using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IFacturaRepositorio
{
    List<Factura> Listar(string filtro, int? idCliente, int numeroPagina, int tamanioPagina);

    int ContarFacturas(string filtro, int? idCliente);

    Factura GetPorId(int idFactura);

    int ContarFacturasDelAnio(int anio);

    int Insertar(Factura factura);
}

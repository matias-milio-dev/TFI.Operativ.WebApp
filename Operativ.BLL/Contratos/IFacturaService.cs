using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.WebServices.Modelos;

namespace Operativ.BLL.Contratos;
public interface IFacturaService
{
    List<Factura> ListarFacturas(string filtro, int? idCliente, int numeroPagina, int tamanioPagina);

    int ContarFacturas(string filtro, int? idCliente);

    Factura ObtenerFacturaPorId(int idFactura, int? idCliente);

    FacturaXml GenerarFactura(int idFactura);

    string GenerarNumeroFactura();

    int EmitirFactura(Suscripcion suscripcion, string numeroFactura);
}

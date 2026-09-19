using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fachadas;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.WebServices.Modelos;

namespace Operativ.BLL.Implementaciones;
public class FacturaService : IFacturaService
{
    private const string PrefijoFactura = "FAC-A";

    private readonly IFacturaRepositorio facturaRepositorio;
    private readonly IBitacoraService bitacoraService;
    private readonly ServicioFacade servicioFacade;

    public FacturaService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        facturaRepositorio = fabricaRepositorio.CrearFacturaRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();

        servicioFacade = new ServicioFacade();
    }

    public List<Factura> ListarFacturas(string filtro, int? idCliente, int numeroPagina, int tamanioPagina)
    {
        return facturaRepositorio.Listar(filtro, idCliente, numeroPagina, tamanioPagina);
    }

    public int ContarFacturas(string filtro, int? idCliente)
    {
        return facturaRepositorio.ContarFacturas(filtro, idCliente);
    }

    public Factura ObtenerFacturaPorId(int idFactura, int? idCliente)
    {
        Factura factura = facturaRepositorio.GetPorId(idFactura);

        if (factura == null || (idCliente.HasValue && factura.IdCliente != idCliente.Value))
        {
            throw new OperativException(TipoError.ErrorFacturaNoExiste);
        }

        return factura;
    }

    public FacturaXml GenerarFactura(int idFactura)
    {
        return servicioFacade.GenerarFactura(idFactura);
    }

    public string GenerarNumeroFactura()
    {
        int anio = DateTime.Now.Year;
        int secuencia = facturaRepositorio.ContarFacturasDelAnio(anio) + 1;

        return PrefijoFactura + "-" + ConfiguracionAplicacion.PuntoVentaFactura + "-" + secuencia.ToString("D8");
    }

    public int EmitirFactura(Suscripcion suscripcion, string numeroFactura)
    {
        Factura factura = new Factura
        {
            NumeroFactura = numeroFactura,
            IdSuscripcion = suscripcion.IdSuscripcion,
            Total = suscripcion.PrecioAnual,
            Moneda = ConfiguracionAplicacion.MonedaFacturacion
        };

        int idFactura = facturaRepositorio.Insertar(factura);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.EmisionFactura, factura.NumeroFactura);

        return idFactura;
    }

    private int? ObtenerIdUsuarioActual()
    {
        SesionHandler sesionHandler = new SesionHandler();
        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario == null)
        {
            return null;
        }

        return usuario.IdUsuario;
    }
}

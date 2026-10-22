using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fachadas;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.SEC.Helpers;
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
        decimal total = CalculoImpuestos.CalcularTotal(suscripcion.PrecioAnual, ConfiguracionAplicacion.PorcentajeIvaFactura);

        Factura factura = new Factura
        {
            NumeroFactura = numeroFactura,
            IdSuscripcion = suscripcion.IdSuscripcion,
            Subtotal = suscripcion.PrecioAnual,
            Total = total,
            Moneda = ConfiguracionAplicacion.MonedaFacturacion
        };

        int idFactura = facturaRepositorio.Insertar(factura);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.EmisionFactura, factura.NumeroFactura);

        return idFactura;
    }

    public bool EnviarFacturaPorEmail(int idFactura)
    {
        Factura factura = facturaRepositorio.GetPorId(idFactura)
            ?? throw new OperativException(TipoError.ErrorFacturaNoExiste);

        if (!ConfiguracionAplicacion.HabilitarEnvioEmail)
        {
            return false;
        }

        try
        {
            FacturaXml facturaXml = GenerarFactura(idFactura);
            string cuerpoHtml = ArmarCuerpoCorreoFactura(factura);

            EmailHelper.EnviarFactura(factura.EmailCliente, factura.NumeroFactura, cuerpoHtml, facturaXml.FacturaHtml);
        }
        catch (Exception)
        {
            bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.EnvioFacturaEmailFallido, factura.NumeroFactura);
            return false;
        }

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.EnvioFacturaEmail, factura.NumeroFactura + " a " + factura.EmailCliente);
        return true;
    }

    private string ArmarCuerpoCorreoFactura(Factura factura)
    {
        CultureInfo cultura = CultureInfo.InvariantCulture;
        string moneda = WebUtility.HtmlEncode(factura.Moneda);

        StringBuilder cuerpo = new StringBuilder();
        cuerpo.Append("<div style=\"font-family:Arial,sans-serif;color:#222;max-width:560px\">");
        cuerpo.Append("<h2 style=\"margin:0 0 8px\">Operativ - Factura ").Append(WebUtility.HtmlEncode(factura.NumeroFactura)).Append("</h2>");
        cuerpo.Append("<p>Hola ").Append(WebUtility.HtmlEncode(factura.RazonSocialCliente)).Append(", gracias por su compra. Estos son los datos:</p>");
        cuerpo.Append("<table style=\"border-collapse:collapse;width:100%\">");
        AgregarFilaCorreo(cuerpo, "Fecha de emisión", factura.FechaEmision.ToString("dd/MM/yyyy", cultura));
        AgregarFilaCorreo(cuerpo, "CUIT", factura.CuitCliente);
        AgregarFilaCorreo(cuerpo, "Plan", factura.NombrePlan);
        AgregarFilaCorreo(cuerpo, "Comprobante de pago", factura.CodigoComprobantePago);
        AgregarFilaCorreo(cuerpo, "Subtotal", moneda + " " + factura.Subtotal.ToString("N2", cultura));
        AgregarFilaCorreo(cuerpo, "IVA " + factura.PorcentajeIva.ToString("0.##", cultura) + "%", moneda + " " + factura.Iva.ToString("N2", cultura));
        AgregarFilaCorreo(cuerpo, "TOTAL", moneda + " " + factura.Total.ToString("N2", cultura));
        cuerpo.Append("</table>");
        cuerpo.Append("<p>Adjuntamos la factura completa. Puede imprimirla o guardarla como PDF desde el navegador.</p>");
        cuerpo.Append("<p>Operativ.</p></div>");

        return cuerpo.ToString();
    }

    private void AgregarFilaCorreo(StringBuilder cuerpo, string etiqueta, string valor)
    {
        cuerpo.Append("<tr><td style=\"padding:6px;border-bottom:1px solid #ddd;color:#666\">").Append(WebUtility.HtmlEncode(etiqueta));
        cuerpo.Append("</td><td style=\"padding:6px;border-bottom:1px solid #ddd;text-align:right\">").Append(WebUtility.HtmlEncode(valor ?? "-"));
        cuerpo.Append("</td></tr>");
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

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.WebServices.Modelos;

namespace Operativ.Web.Paginas;
public partial class ConsultaFacturas : PaginaSeguraBase
{
    private const string ComandoVer = "Ver";

    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaFacturas;
    private readonly IFacturaService facturaService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.ConsultarFacturas }; }
    }

    public ConsultaFacturas()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        facturaService = fabricaNegocio.CrearFacturaService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ucPaginador.TamanioPagina = tamanioPagina;

        CargarGrilla();
    }

    protected void ucPaginador_PaginaCambiada(object sender, EventArgs e)
    {
        CargarGrilla();
    }

    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        ucPaginador.Reiniciar();
        CargarGrilla();
    }

    protected void btnCerrarDetalle_Click(object sender, EventArgs e)
    {
        pnlDetalleFactura.Visible = false;
    }

    protected void gvFacturas_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != ComandoVer)
        {
            return;
        }

        try
        {
            int idFactura = Convert.ToInt32(e.CommandArgument);

            Factura factura = facturaService.ObtenerFacturaPorId(idFactura, ObtenerIdClienteSesion());
            FacturaXml facturaXml = facturaService.GenerarFactura(factura.IdFactura);

            litFacturaHtml.Text = facturaXml.FacturaHtml;
            lnkDescargarPdfDetalle.NavigateUrl = NavegacionHelper.ObtenerUrlExportacionFactura(factura.IdFactura);
            pnlDetalleFactura.Visible = true;
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void CargarGrilla()
    {
        string filtro = txtFiltro.Text.Trim();
        int? idCliente = ObtenerIdClienteSesion();

        List<Factura> facturas = facturaService.ListarFacturas(filtro, idCliente, ucPaginador.NumeroPagina, tamanioPagina);
        int total = facturaService.ContarFacturas(filtro, idCliente);

        gvFacturas.DataSource = facturas;
        gvFacturas.DataBind();

        ucPaginador.Actualizar(total, facturas.Count);
    }
}

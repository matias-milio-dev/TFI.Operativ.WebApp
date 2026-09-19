using System;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;
using Operativ.WebServices.Modelos;

namespace Operativ.Web.Paginas;
public partial class ImprimirFactura : PaginaSeguraBase
{
    private const string ParametroIdFactura = "id";
    private const string PrefijoTitulo = "factura_";

    private readonly IFacturaService facturaService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.ConsultarFacturas }; }
    }

    public ImprimirFactura()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        facturaService = fabricaNegocio.CrearFacturaService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            int idFactura = ObtenerIdFacturaSolicitada();

            Factura factura = facturaService.ObtenerFacturaPorId(idFactura, ObtenerIdClienteSesion());
            FacturaXml facturaXml = facturaService.GenerarFactura(factura.IdFactura);

            litFacturaHtml.Text = facturaXml.FacturaHtml;
            Title = PrefijoTitulo + factura.NumeroFactura;
            pnlBarraImpresion.Visible = true;

            if (!IsPostBack)
            {
                ClientScript.RegisterStartupScript(GetType(), "ImprimirFactura", "Operativ.imprimirDocumentoAlCargar();", true);
            }
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private int ObtenerIdFacturaSolicitada()
    {
        int idFactura;

        if (!int.TryParse(Request.QueryString[ParametroIdFactura], out idFactura))
        {
            throw new OperativException(TipoError.ErrorFacturaNoExiste);
        }

        return idFactura;
    }
}

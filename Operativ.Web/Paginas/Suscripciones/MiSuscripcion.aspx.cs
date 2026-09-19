using System;
using System.Web;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;
using Operativ.Web.Idioma;
using Operativ.WebServices.Modelos;

namespace Operativ.Web.Paginas;
public partial class MiSuscripcion : PaginaSeguraBase
{
    private readonly ISuscripcionService suscripcionService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.GestionarSuscripciones }; }
    }

    public MiSuscripcion()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        suscripcionService = fabricaNegocio.CrearSuscripcionService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            MostrarEstadoActual();
        }
    }

    protected void lnkContratar_Command(object sender, CommandEventArgs e)
    {
        try
        {
            int idCliente = ObtenerIdClienteObligatorio();
            int idPlan = Convert.ToInt32(e.CommandArgument);

            int idSuscripcion = suscripcionService.AltaSuscripcion(idCliente, idPlan);

            ControlNotificaciones.MostrarExito("MensajeExitoAltaSuscripcion");

            MostrarResumen(idSuscripcion);
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
            MostrarEstadoActual();
        }
    }

    protected void btnVerResumen_Click(object sender, EventArgs e)
    {
        try
        {
            Suscripcion suscripcion = ObtenerVigenteObligatoria();

            MostrarResumen(suscripcion.IdSuscripcion);
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
            MostrarEstadoActual();
        }
    }

    protected void btnCancelarSuscripcion_Click(object sender, EventArgs e)
    {
        try
        {
            Suscripcion suscripcion = ObtenerVigenteObligatoria();

            suscripcionService.CancelarSuscripcion(suscripcion.IdSuscripcion);

            ControlNotificaciones.MostrarExito("MensajeExitoCancelacionSuscripcion");
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }

        MostrarEstadoActual();
    }

    protected void btnVolverEstado_Click(object sender, EventArgs e)
    {
        MostrarEstadoActual();
    }

    private void MostrarEstadoActual()
    {
        pnlResumen.Visible = false;

        int? idCliente = ObtenerIdClienteSesion();

        if (!idCliente.HasValue)
        {
            ControlNotificaciones.MostrarMensaje(TipoError.ErrorUsuarioClienteSinEmpresa);
            pnlSinSuscripcion.Visible = false;
            pnlEstadoSuscripcion.Visible = false;
            return;
        }

        Suscripcion suscripcion = suscripcionService.ObtenerSuscripcionVigente(idCliente.Value);

        if (suscripcion == null || !suscripcion.EsVigente)
        {
            MostrarPlanes();
            return;
        }

        MostrarDetalleSuscripcion(suscripcion);
    }

    private void MostrarPlanes()
    {
        rptPlanes.DataSource = suscripcionService.ListarPlanes();
        rptPlanes.DataBind();

        pnlSinSuscripcion.Visible = true;
        pnlEstadoSuscripcion.Visible = false;
    }

    private void MostrarDetalleSuscripcion(Suscripcion suscripcion)
    {
        litNombrePlan.Text = HttpUtility.HtmlEncode(suscripcion.NombrePlan);
        litPrecioAnual.Text = suscripcion.PrecioAnual.ToString("N2");
        lblEstado.Text = TextoRecurso.Obtener("EtiquetaEstadoSuscripcion" + suscripcion.Estado.ToString());
        lblEstado.CssClass = "badge badge-suscripcion-" + suscripcion.Estado.ToString().ToLowerInvariant();
        litFechaAlta.Text = suscripcion.FechaAlta.ToString("dd/MM/yyyy");
        litFinTrial.Text = suscripcion.FechaFinTrial.ToString("dd/MM/yyyy");
        litVencimiento.Text = suscripcion.FechaVencimiento.HasValue ? suscripcion.FechaVencimiento.Value.ToString("dd/MM/yyyy") : "-";
        litComprobante.Text = string.IsNullOrEmpty(suscripcion.CodigoComprobante) ? "-" : HttpUtility.HtmlEncode(suscripcion.CodigoComprobante);

        btnVerResumen.Visible = suscripcion.EstaPendienteDePago;

        pnlSinSuscripcion.Visible = false;
        pnlEstadoSuscripcion.Visible = true;
    }

    private void MostrarResumen(int idSuscripcion)
    {
        ResumenSuscripcionXml resumen = suscripcionService.GenerarResumen(idSuscripcion);

        litResumenHtml.Text = resumen.ResumenHtml;
        litArchivoXml.Text = TextoRecurso.Formato("MensajeArchivoResumenGenerado", HttpUtility.HtmlEncode(resumen.NombreArchivoXml));

        pnlSinSuscripcion.Visible = false;
        pnlEstadoSuscripcion.Visible = false;
        pnlResumen.Visible = true;
    }

    private int ObtenerIdClienteObligatorio()
    {
        int? idCliente = ObtenerIdClienteSesion();

        if (!idCliente.HasValue)
        {
            throw new OperativException(TipoError.ErrorUsuarioClienteSinEmpresa);
        }

        return idCliente.Value;
    }

    private Suscripcion ObtenerVigenteObligatoria()
    {
        int idCliente = ObtenerIdClienteObligatorio();

        Suscripcion suscripcion = suscripcionService.ObtenerSuscripcionVigente(idCliente)
            ?? throw new OperativException(TipoError.ErrorSuscripcionNoExiste);

        return suscripcion;
    }
}

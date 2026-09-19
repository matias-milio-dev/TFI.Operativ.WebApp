using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class GestionIncidentes : PaginaSeguraBase
{
    private const string ComandoVer = "Ver";
    private const string ComandoCerrar = "Cerrar";

    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaIncidentes;
    private readonly IIncidenteService incidenteService;
    private readonly IActivoService activoService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.ReportarIncidentes, NombrePatente.CerrarIncidente }; }
    }

    public GestionIncidentes()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        incidenteService = fabricaNegocio.CrearIncidenteService();
        activoService = fabricaNegocio.CrearActivoService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ucPaginador.TamanioPagina = tamanioPagina;

        if (!IsPostBack)
        {
            CargarFiltroEstado();
            CargarCategorias();
            CargarPrioridades();
        }

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

    protected void btnNuevoIncidente_Click(object sender, EventArgs e)
    {
        try
        {
            PrepararAlta();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void btnCancelarAlta_Click(object sender, EventArgs e)
    {
        pnlFormularioIncidente.Visible = false;
    }

    protected void btnCerrarDetalle_Click(object sender, EventArgs e)
    {
        OcultarDetalle();
    }

    protected void gvIncidentes_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int idIncidente = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == ComandoVer)
        {
            MostrarDetalle(idIncidente, false);
        }
        else if (e.CommandName == ComandoCerrar)
        {
            MostrarDetalle(idIncidente, true);
        }
    }

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        try
        {
            int? idCliente = ObtenerIdClienteSesion();

            if (!idCliente.HasValue)
            {
                throw new OperativException(TipoError.ErrorUsuarioClienteSinEmpresa);
            }

            Incidente incidente = new Incidente
            {
                IdActivo = Convert.ToInt32(ddlActivo.SelectedValue),
                Descripcion = txtDescripcion.Text.Trim(),
                Categoria = (CategoriaIncidente)Enum.Parse(typeof(CategoriaIncidente), ddlCategoria.SelectedValue),
                Prioridad = (PrioridadIncidente)Enum.Parse(typeof(PrioridadIncidente), ddlPrioridad.SelectedValue)
            };

            incidenteService.AltaIncidente(incidente, idCliente.Value);

            ControlNotificaciones.MostrarExito("MensajeExitoAltaIncidente", incidente.NumeroIncidente);

            pnlFormularioIncidente.Visible = false;
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void btnConfirmarCierre_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        try
        {
            int idIncidente = Convert.ToInt32(hidIdIncidente.Value);

            incidenteService.CerrarIncidente(idIncidente, txtComentarioResolucion.Text.Trim());

            ControlNotificaciones.MostrarExito("MensajeExitoCierreIncidente");

            OcultarDetalle();
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected string ObtenerClaseEstadoIncidente(EstadoIncidente estado)
    {
        return "badge-incidente-" + estado.ToString().ToLowerInvariant();
    }

    protected string ObtenerTextoEstadoIncidente(EstadoIncidente estado)
    {
        return TextoRecurso.Obtener("EtiquetaEstadoIncidente" + estado.ToString());
    }

    protected string ObtenerClasePrioridad(PrioridadIncidente prioridad)
    {
        return "badge-prioridad-" + prioridad.ToString().ToLowerInvariant();
    }

    protected string ObtenerTextoPrioridad(PrioridadIncidente prioridad)
    {
        return TextoRecurso.Obtener("EtiquetaPrioridadIncidente" + prioridad.ToString());
    }

    private void PrepararAlta()
    {
        int? idCliente = ObtenerIdClienteSesion();

        if (!idCliente.HasValue)
        {
            throw new OperativException(TipoError.ErrorUsuarioClienteSinEmpresa);
        }

        List<Activo> activos = activoService.ListarActivosDeCliente(idCliente.Value);

        if (activos.Count == 0)
        {
            throw new OperativException(TipoError.ErrorSinActivosParaIncidente);
        }

        CargarActivos(activos);

        txtDescripcion.Text = string.Empty;
        ddlCategoria.SelectedIndex = 0;
        ddlPrioridad.SelectedIndex = 0;

        OcultarDetalle();
        pnlFormularioIncidente.Visible = true;
    }

    private void MostrarDetalle(int idIncidente, bool prepararCierre)
    {
        try
        {
            Incidente incidente = incidenteService.ObtenerIncidentePorId(idIncidente);

            hidIdIncidente.Value = incidente.IdIncidente.ToString();
            tituloDetalle.InnerText = incidente.NumeroIncidente;

            litActivo.Text = HttpUtility.HtmlEncode(incidente.NombreActivo + " (" + incidente.NumeroSerieActivo + ")");
            litCliente.Text = HttpUtility.HtmlEncode(incidente.RazonSocialCliente);
            litCategoria.Text = TextoRecurso.Obtener("EtiquetaCategoriaIncidente" + incidente.Categoria.ToString());
            litPrioridad.Text = ObtenerTextoPrioridad(incidente.Prioridad);
            litEstado.Text = ObtenerTextoEstadoIncidente(incidente.Estado);
            litFechaAlta.Text = incidente.FechaAlta.ToString("dd/MM/yyyy HH:mm");
            litDescripcion.Text = HttpUtility.HtmlEncode(incidente.Descripcion);

            pnlResolucion.Visible = incidente.EstaCerrado;

            if (incidente.EstaCerrado)
            {
                litFechaCierre.Text = incidente.FechaCierre.Value.ToString("dd/MM/yyyy HH:mm");
                litComentarioResolucion.Text = HttpUtility.HtmlEncode(incidente.ComentarioResolucion);
            }

            bool mostrarCierre = prepararCierre && !incidente.EstaCerrado;

            pnlCierre.Visible = mostrarCierre;
            rfvComentarioResolucion.Enabled = mostrarCierre;
            txtComentarioResolucion.Text = string.Empty;

            pnlFormularioIncidente.Visible = false;
            pnlDetalleIncidente.Visible = true;
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void OcultarDetalle()
    {
        pnlDetalleIncidente.Visible = false;
        pnlCierre.Visible = false;
        rfvComentarioResolucion.Enabled = false;
    }

    private void CargarActivos(List<Activo> activos)
    {
        ddlActivo.Items.Clear();
        ddlActivo.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaActivoPlaceholder"), string.Empty));

        foreach (Activo activo in activos)
        {
            ddlActivo.Items.Add(new ListItem(activo.Nombre + " - " + activo.NumeroSerie, activo.IdActivo.ToString()));
        }
    }

    private void CargarFiltroEstado()
    {
        ddlFiltroEstado.Items.Clear();
        ddlFiltroEstado.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaTodosLosEstados"), string.Empty));

        foreach (EstadoIncidente estado in Enum.GetValues(typeof(EstadoIncidente)))
        {
            ddlFiltroEstado.Items.Add(new ListItem(ObtenerTextoEstadoIncidente(estado), estado.ToString()));
        }
    }

    private void CargarCategorias()
    {
        ddlCategoria.Items.Clear();

        foreach (CategoriaIncidente categoria in Enum.GetValues(typeof(CategoriaIncidente)))
        {
            string texto = TextoRecurso.Obtener("EtiquetaCategoriaIncidente" + categoria.ToString());
            ddlCategoria.Items.Add(new ListItem(texto, categoria.ToString()));
        }
    }

    private void CargarPrioridades()
    {
        ddlPrioridad.Items.Clear();

        foreach (PrioridadIncidente prioridad in Enum.GetValues(typeof(PrioridadIncidente)))
        {
            ddlPrioridad.Items.Add(new ListItem(ObtenerTextoPrioridad(prioridad), prioridad.ToString()));
        }
    }

    private EstadoIncidente? ObtenerEstadoFiltro()
    {
        if (string.IsNullOrEmpty(ddlFiltroEstado.SelectedValue))
        {
            return null;
        }

        return (EstadoIncidente)Enum.Parse(typeof(EstadoIncidente), ddlFiltroEstado.SelectedValue);
    }

    private void CargarGrilla()
    {
        string filtro = txtFiltro.Text.Trim();
        EstadoIncidente? estado = ObtenerEstadoFiltro();
        int? idCliente = ObtenerIdClienteSesion();

        List<Incidente> incidentes = incidenteService.ListarIncidentes(filtro, estado, idCliente, ucPaginador.NumeroPagina, tamanioPagina);
        int total = incidenteService.ContarIncidentes(filtro, estado, idCliente);

        gvIncidentes.DataSource = incidentes;
        gvIncidentes.DataBind();

        ucPaginador.Actualizar(total, incidentes.Count);
    }
}

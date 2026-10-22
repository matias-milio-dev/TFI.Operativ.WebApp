using System;
using System.Collections.Generic;
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
public partial class MonitoreoActivos : PaginaSeguraBase
{
    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaMonitoreo;
    private readonly IActivoService activoService;
    private readonly IPaqueteService paqueteService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.MonitorearActivos }; }
    }

    public MonitoreoActivos()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        activoService = fabricaNegocio.CrearActivoService();
        paqueteService = fabricaNegocio.CrearPaqueteService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ucPaginador.TamanioPagina = tamanioPagina;

        try
        {
            if (!IsPostBack)
            {
                CargarFiltroEstado();
                CargarFiltroPaquete();
            }

            CargarDatos();
        }
        catch (Exception excepcion)
        {
            pnlMonitoreo.Visible = false;
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void ucPaginador_PaginaCambiada(object sender, EventArgs e)
    {
        CargarDatos();
    }

    protected void btnFiltrar_Click(object sender, EventArgs e)
    {
        ucPaginador.Reiniciar();
        CargarDatos();
    }

    protected void btnLimpiar_Click(object sender, EventArgs e)
    {
        txtFiltro.Text = string.Empty;
        ddlFiltroEstado.SelectedIndex = 0;
        ddlFiltroPaquete.SelectedIndex = 0;

        ucPaginador.Reiniciar();
        CargarDatos();
    }

    protected string ObtenerClaseEstado(EstadoActivo estado)
    {
        return "badge-estado-" + estado.ToString().ToLowerInvariant();
    }

    protected string ObtenerTextoEstado(EstadoActivo estado)
    {
        return TextoRecurso.Obtener("EtiquetaEstadoActivo" + estado.ToString());
    }

    private void CargarFiltroEstado()
    {
        ddlFiltroEstado.Items.Clear();
        ddlFiltroEstado.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaTodosLosEstados"), string.Empty));

        foreach (EstadoActivo estado in Enum.GetValues(typeof(EstadoActivo)))
        {
            ddlFiltroEstado.Items.Add(new ListItem(ObtenerTextoEstado(estado), estado.ToString()));
        }
    }

    private void CargarFiltroPaquete()
    {
        List<Paquete> paquetes = paqueteService.ListarPaquetesHabilitados(null);

        ddlFiltroPaquete.Items.Clear();
        ddlFiltroPaquete.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaTodosLosPaquetes"), string.Empty));

        foreach (Paquete paquete in paquetes)
        {
            ddlFiltroPaquete.Items.Add(new ListItem(paquete.Nombre, paquete.IdPaquete.ToString()));
        }
    }

    private void CargarDatos()
    {
        int idCliente = ObtenerIdClienteObligatorio();
        string filtro = txtFiltro.Text.Trim();
        EstadoActivo? estado = ObtenerEstadoFiltrado();
        int? idPaquete = ObtenerPaqueteFiltrado();

        // El resumen ignora los filtros: siempre muestra el total de la empresa.
        ResumenActivos resumen = activoService.ObtenerResumenActivos(idCliente);

        litTotal.Text = resumen.Total.ToString();
        litDisponibles.Text = resumen.Disponibles.ToString();
        litAsignados.Text = resumen.Asignados.ToString();
        litEnReparacion.Text = resumen.EnReparacion.ToString();

        List<Activo> activos = activoService.ListarActivos(filtro, idCliente, estado, idPaquete, ucPaginador.NumeroPagina, tamanioPagina);
        int total = activoService.ContarActivos(filtro, idCliente, estado, idPaquete);

        gvActivos.DataSource = activos;
        gvActivos.DataBind();

        ucPaginador.Actualizar(total, activos.Count);
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

    private EstadoActivo? ObtenerEstadoFiltrado()
    {
        if (string.IsNullOrEmpty(ddlFiltroEstado.SelectedValue))
        {
            return null;
        }

        return (EstadoActivo)Enum.Parse(typeof(EstadoActivo), ddlFiltroEstado.SelectedValue);
    }

    private int? ObtenerPaqueteFiltrado()
    {
        if (string.IsNullOrEmpty(ddlFiltroPaquete.SelectedValue))
        {
            return null;
        }

        return Convert.ToInt32(ddlFiltroPaquete.SelectedValue);
    }
}

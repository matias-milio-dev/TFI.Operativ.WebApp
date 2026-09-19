using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class GestionActivos : PaginaSeguraBase
{
    private const string ComandoEditar = "Editar";
    private const string ComandoBaja = "Baja";

    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaActivos;
    private readonly IActivoService activoService;
    private readonly IPaqueteService paqueteService;
    private readonly IClienteService clienteService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.GestionarActivos }; }
    }

    public GestionActivos()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        activoService = fabricaNegocio.CrearActivoService();
        paqueteService = fabricaNegocio.CrearPaqueteService();
        clienteService = fabricaNegocio.CrearClienteService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ucPaginador.TamanioPagina = tamanioPagina;

        if (!IsPostBack)
        {
            CargarEstados();
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

    protected void btnNuevoActivo_Click(object sender, EventArgs e)
    {
        PrepararAlta();
    }

    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        pnlFormularioActivo.Visible = false;
    }

    protected void gvActivos_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int idActivo = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == ComandoEditar)
        {
            CargarActivoParaEdicion(idActivo);
        }
        else if (e.CommandName == ComandoBaja)
        {
            DarDeBaja(idActivo);
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
            int idActivo = Convert.ToInt32(hidIdActivo.Value);

            Activo activo = new Activo
            {
                IdActivo = idActivo,
                Nombre = txtNombre.Text.Trim(),
                Modelo = txtModelo.Text.Trim(),
                NumeroSerie = txtNumeroSerie.Text.Trim(),
                Especificaciones = txtEspecificaciones.Text.Trim(),
                IdPaquete = Convert.ToInt32(ddlPaquete.SelectedValue),
                IdCliente = Convert.ToInt32(ddlCliente.SelectedValue),
                Estado = ObtenerEstadoSeleccionado()
            };

            if (idActivo == 0)
            {
                activoService.AltaActivo(activo);
                ControlNotificaciones.MostrarExito("MensajeExitoAltaActivo");
            }
            else
            {
                activoService.ModificarActivo(activo);
                ControlNotificaciones.MostrarExito("MensajeExitoModificacionActivo");
            }

            pnlFormularioActivo.Visible = false;
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected string ObtenerClaseEstado(EstadoActivo estado)
    {
        return "badge-estado-" + estado.ToString().ToLowerInvariant();
    }

    protected string ObtenerTextoEstado(EstadoActivo estado)
    {
        return TextoRecurso.Obtener("EtiquetaEstadoActivo" + estado.ToString());
    }

    private void DarDeBaja(int idActivo)
    {
        try
        {
            activoService.BajaActivo(idActivo);
            ControlNotificaciones.MostrarExito("MensajeExitoBajaActivo");

            pnlFormularioActivo.Visible = false;
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void CargarActivoParaEdicion(int idActivo)
    {
        try
        {
            Activo activo = activoService.ObtenerActivoPorId(idActivo);

            CargarPaquetes(activo.IdPaquete);
            CargarClientes(activo.IdCliente);

            hidIdActivo.Value = activo.IdActivo.ToString();
            txtNombre.Text = activo.Nombre;
            txtModelo.Text = activo.Modelo;
            txtNumeroSerie.Text = activo.NumeroSerie;
            txtEspecificaciones.Text = activo.Especificaciones;
            ddlPaquete.SelectedValue = activo.IdPaquete.ToString();
            ddlCliente.SelectedValue = activo.IdCliente.ToString();
            ddlEstado.SelectedValue = activo.Estado.ToString();

            tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioModificacionActivo");
            pnlFormularioActivo.Visible = true;
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void PrepararAlta()
    {
        CargarPaquetes(null);
        CargarClientes(null);

        hidIdActivo.Value = "0";
        txtNombre.Text = string.Empty;
        txtModelo.Text = string.Empty;
        txtNumeroSerie.Text = string.Empty;
        txtEspecificaciones.Text = string.Empty;
        ddlEstado.SelectedValue = EstadoActivo.Disponible.ToString();

        tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioAltaActivo");
        pnlFormularioActivo.Visible = true;
    }

    private void CargarPaquetes(int? idPaqueteIncluir)
    {
        List<Paquete> paquetes = paqueteService.ListarPaquetesHabilitados(idPaqueteIncluir);

        ddlPaquete.Items.Clear();
        ddlPaquete.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaPaquetePlaceholder"), string.Empty));

        foreach (Paquete paquete in paquetes)
        {
            ddlPaquete.Items.Add(new ListItem(paquete.Nombre, paquete.IdPaquete.ToString()));
        }
    }

    private void CargarClientes(int? idClienteIncluir)
    {
        List<Cliente> clientes = clienteService.ListarClientesActivos(idClienteIncluir);

        ddlCliente.Items.Clear();
        ddlCliente.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaEmpresaPlaceholder"), string.Empty));

        foreach (Cliente cliente in clientes)
        {
            ddlCliente.Items.Add(new ListItem(cliente.RazonSocial, cliente.IdCliente.ToString()));
        }
    }

    private void CargarEstados()
    {
        ddlEstado.Items.Clear();

        foreach (EstadoActivo estado in Enum.GetValues(typeof(EstadoActivo)))
        {
            ddlEstado.Items.Add(new ListItem(ObtenerTextoEstado(estado), estado.ToString()));
        }
    }

    private EstadoActivo ObtenerEstadoSeleccionado()
    {
        return (EstadoActivo)Enum.Parse(typeof(EstadoActivo), ddlEstado.SelectedValue);
    }

    private void CargarGrilla()
    {
        string filtro = txtFiltro.Text.Trim();

        List<Activo> activos = activoService.ListarActivos(filtro, ObtenerIdClienteSesion(), ucPaginador.NumeroPagina, tamanioPagina);
        int total = activoService.ContarActivos(filtro, ObtenerIdClienteSesion());

        gvActivos.DataSource = activos;
        gvActivos.DataBind();

        ucPaginador.Actualizar(total, activos.Count);
    }
}

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class ConsultarBitacora : PaginaSeguraBase
{
    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaBitacora;
    private readonly IBitacoraService bitacoraService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.ConsultarBitacora }; }
    }

    public ConsultarBitacora()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ucPaginador.TamanioPagina = tamanioPagina;

        if (!IsPostBack)
        {
            CargarFiltros();
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

    protected string ObtenerClaseCriticidad(CriticidadBitacora criticidad)
    {
        return "badge-" + criticidad.ToString().ToLowerInvariant();
    }

    protected string ObtenerTextoCriticidad(CriticidadBitacora criticidad)
    {
        return TextoRecurso.Obtener("EtiquetaCriticidad" + criticidad.ToString());
    }

    private void CargarFiltros()
    {
        ddlFiltroAccion.Items.Clear();
        ddlFiltroAccion.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaTodasLasAcciones"), string.Empty));

        foreach (AccionBitacora accion in AccionBitacora.ObtenerTodas())
        {
            ddlFiltroAccion.Items.Add(new ListItem(accion.Descripcion.Replace("{0}", "N"), accion.Tipo.ToString()));
        }

        ddlFiltroCriticidad.Items.Clear();
        ddlFiltroCriticidad.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaTodasLasCriticidades"), string.Empty));

        foreach (CriticidadBitacora criticidad in Enum.GetValues(typeof(CriticidadBitacora)))
        {
            ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(criticidad), criticidad.ToString()));
        }
    }

    private void CargarGrilla()
    {
        string filtroUsuario = txtFiltroUsuario.Text.Trim();
        TipoAccionBitacora? accion = ObtenerAccionSeleccionada();
        CriticidadBitacora? criticidad = ObtenerCriticidadSeleccionada();
        DateTime? fechaDesde = ObtenerFecha(txtFechaDesde.Text);
        DateTime? fechaHasta = ObtenerFecha(txtFechaHasta.Text);

        List<Bitacora> registros = bitacoraService.Buscar(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta, ucPaginador.NumeroPagina, tamanioPagina);
        int total = bitacoraService.ContarRegistros(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta);

        gvBitacora.DataSource = registros;
        gvBitacora.DataBind();

        ucPaginador.Actualizar(total, registros.Count);
    }

    private TipoAccionBitacora? ObtenerAccionSeleccionada()
    {
        if (string.IsNullOrEmpty(ddlFiltroAccion.SelectedValue))
        {
            return null;
        }

        return (TipoAccionBitacora)Enum.Parse(typeof(TipoAccionBitacora), ddlFiltroAccion.SelectedValue);
    }

    private CriticidadBitacora? ObtenerCriticidadSeleccionada()
    {
        if (string.IsNullOrEmpty(ddlFiltroCriticidad.SelectedValue))
        {
            return null;
        }

        return (CriticidadBitacora)Enum.Parse(typeof(CriticidadBitacora), ddlFiltroCriticidad.SelectedValue);
    }

    private DateTime? ObtenerFecha(string texto)
    {
        DateTime fecha;

        if (!DateTime.TryParse(texto, out fecha))
        {
            return null;
        }

        return fecha;
    }
}

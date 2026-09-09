using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class ConsultarBitacora : PaginaSeguraBase
{
    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaBitacora;
    private readonly IBitacoraService bitacoraService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.ConsultarBitacora }; }
    }

    private int NumeroPagina
    {
        get { return ViewState["NumeroPagina"] == null ? 1 : (int)ViewState["NumeroPagina"]; }
        set { ViewState["NumeroPagina"] = value; }
    }

    public ConsultarBitacora()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarFiltros();
        }

        CargarGrilla();
    }

    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        NumeroPagina = 1;
        CargarGrilla();
    }

    protected void btnPaginaAnterior_Click(object sender, EventArgs e)
    {
        if (NumeroPagina > 1)
        {
            NumeroPagina--;
        }

        CargarGrilla();
    }

    protected void btnPaginaSiguiente_Click(object sender, EventArgs e)
    {
        NumeroPagina++;
        CargarGrilla();
    }

    protected string ObtenerClaseCriticidad(CriticidadBitacora criticidad)
    {
        switch (criticidad)
        {
            case CriticidadBitacora.Informativo:
                return "badge-informativo";
            case CriticidadBitacora.Advertencia:
                return "badge-advertencia";
            case CriticidadBitacora.Critico:
                return "badge-critico";
            case CriticidadBitacora.Grave:
                return "badge-grave";
            default:
                return "badge-informativo";
        }
    }

    protected string ObtenerTextoCriticidad(CriticidadBitacora criticidad)
    {
        switch (criticidad)
        {
            case CriticidadBitacora.Informativo:
                return (string)GetGlobalResourceObject("Textos", "EtiquetaCriticidadInformativo");
            case CriticidadBitacora.Advertencia:
                return (string)GetGlobalResourceObject("Textos", "EtiquetaCriticidadAdvertencia");
            case CriticidadBitacora.Critico:
                return (string)GetGlobalResourceObject("Textos", "EtiquetaCriticidadCritico");
            case CriticidadBitacora.Grave:
                return (string)GetGlobalResourceObject("Textos", "EtiquetaCriticidadGrave");
            default:
                return criticidad.ToString();
        }
    }

    private void CargarFiltros()
    {
        ddlFiltroAccion.Items.Clear();
        ddlFiltroAccion.Items.Add(new ListItem((string)GetGlobalResourceObject("Textos", "EtiquetaTodasLasAcciones"), string.Empty));

        foreach (AccionBitacora accion in AccionBitacora.ObtenerTodas())
        {
            string texto = accion.Descripcion.Replace("{0}", "N");
            ddlFiltroAccion.Items.Add(new ListItem(texto, accion.Tipo.ToString()));
        }

        ddlFiltroCriticidad.Items.Clear();
        ddlFiltroCriticidad.Items.Add(new ListItem((string)GetGlobalResourceObject("Textos", "EtiquetaTodasLasCriticidades"), string.Empty));
        ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(CriticidadBitacora.Informativo), CriticidadBitacora.Informativo.ToString()));
        ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(CriticidadBitacora.Advertencia), CriticidadBitacora.Advertencia.ToString()));
        ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(CriticidadBitacora.Critico), CriticidadBitacora.Critico.ToString()));
        ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(CriticidadBitacora.Grave), CriticidadBitacora.Grave.ToString()));
    }

    private void CargarGrilla()
    {
        string filtroUsuario = txtFiltroUsuario.Text.Trim();
        TipoAccionBitacora? accion = ObtenerAccionSeleccionada();
        CriticidadBitacora? criticidad = ObtenerCriticidadSeleccionada();
        DateTime? fechaDesde = ObtenerFecha(txtFechaDesde.Text);
        DateTime? fechaHasta = ObtenerFecha(txtFechaHasta.Text);

        List<Bitacora> registros = bitacoraService.Buscar(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta, NumeroPagina, tamanioPagina);
        int total = bitacoraService.ContarRegistros(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta);

        gvBitacora.DataSource = registros;
        gvBitacora.DataBind();

        ActualizarResumenPaginado(total, registros.Count);
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

        if (DateTime.TryParse(texto, out fecha))
        {
            return fecha;
        }

        return null;
    }

    private void ActualizarResumenPaginado(int total, int cantidadEnPagina)
    {
        int desde = total == 0 ? 0 : ((NumeroPagina - 1) * tamanioPagina) + 1;
        int hasta = total == 0 ? 0 : desde + cantidadEnPagina - 1;

        string formato = (string)GetGlobalResourceObject("Textos", "MensajeResumenPaginadoBitacora");
        litResumenPaginado.Text = string.Format(formato, desde, hasta, total);
        litNumeroPagina.Text = NumeroPagina.ToString();

        btnPaginaAnterior.Enabled = NumeroPagina > 1;
        btnPaginaSiguiente.Enabled = (NumeroPagina * tamanioPagina) < total;
    }
}

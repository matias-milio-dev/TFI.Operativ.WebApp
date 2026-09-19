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
public partial class GestionPaquetes : PaginaSeguraBase
{
    private const string ComandoEditar = "Editar";
    private const string ComandoBaja = "Baja";

    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaPaquetes;
    private readonly IPaqueteService paqueteService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.GestionarCatalogo }; }
    }

    public GestionPaquetes()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        paqueteService = fabricaNegocio.CrearPaqueteService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ucPaginador.TamanioPagina = tamanioPagina;

        if (!IsPostBack)
        {
            CargarTiposPermiso();
            CargarProgramas();
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

    protected void btnNuevoPaquete_Click(object sender, EventArgs e)
    {
        PrepararAlta();
    }

    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        MostrarFormulario(false);
    }

    protected void gvPaquetes_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int idPaquete = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == ComandoEditar)
        {
            CargarPaqueteParaEdicion(idPaquete);
        }
        else if (e.CommandName == ComandoBaja)
        {
            DarDeBaja(idPaquete);
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
            int idPaquete = Convert.ToInt32(hidIdPaquete.Value);

            Paquete paquete = new Paquete
            {
                IdPaquete = idPaquete,
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                TipoPermiso = ObtenerTipoPermisoSeleccionado()
            };

            int[] idsPrograma = ObtenerProgramasSeleccionados();

            if (idPaquete == 0)
            {
                paqueteService.AltaPaquete(paquete, idsPrograma);
                ControlNotificaciones.MostrarExito("MensajeExitoAltaPaquete");
            }
            else
            {
                paqueteService.ModificarPaquete(paquete, idsPrograma);
                ControlNotificaciones.MostrarExito("MensajeExitoModificacionPaquete");
            }

            MostrarFormulario(false);
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected string ObtenerClaseTipoPermiso(TipoPermiso tipoPermiso)
    {
        return "badge-permiso-" + tipoPermiso.ToString().ToLowerInvariant();
    }

    protected string ObtenerTextoTipoPermiso(TipoPermiso tipoPermiso)
    {
        return TextoRecurso.Obtener("EtiquetaTipoPermiso" + tipoPermiso.ToString());
    }

    private void DarDeBaja(int idPaquete)
    {
        try
        {
            paqueteService.BajaPaquete(idPaquete);
            ControlNotificaciones.MostrarExito("MensajeExitoBajaPaquete");

            MostrarFormulario(false);
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void CargarPaqueteParaEdicion(int idPaquete)
    {
        try
        {
            Paquete paquete = paqueteService.ObtenerPaquetePorId(idPaquete);

            hidIdPaquete.Value = paquete.IdPaquete.ToString();
            txtNombre.Text = paquete.Nombre;
            txtDescripcion.Text = paquete.Descripcion;
            ddlTipoPermiso.SelectedValue = paquete.TipoPermiso.ToString();

            MarcarProgramas(paquete.Programas);

            tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioModificacionPaquete");
            MostrarFormulario(true);
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void PrepararAlta()
    {
        hidIdPaquete.Value = "0";
        txtNombre.Text = string.Empty;
        txtDescripcion.Text = string.Empty;
        ddlTipoPermiso.SelectedIndex = 0;

        MarcarProgramas(new List<Programa>());

        tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioAltaPaquete");
        MostrarFormulario(true);
    }

    private void MostrarFormulario(bool mostrar)
    {
        pnlFormularioPaquete.Visible = mostrar;
        pnlPaquetes.Visible = !mostrar;
    }

    private void CargarTiposPermiso()
    {
        ddlTipoPermiso.Items.Clear();

        foreach (TipoPermiso tipoPermiso in Enum.GetValues(typeof(TipoPermiso)))
        {
            ddlTipoPermiso.Items.Add(new ListItem(ObtenerTextoTipoPermiso(tipoPermiso), tipoPermiso.ToString()));
        }
    }

    private void CargarProgramas()
    {
        chkProgramas.Items.Clear();

        foreach (Programa programa in paqueteService.ListarProgramas())
        {
            string urlIcono = ResolveUrl(IconoPrograma.ObtenerRuta(programa.Nombre));
            string texto = "<img class=\"icono-programa\" src=\"" + urlIcono + "\" alt=\"\" /><span>" + Server.HtmlEncode(programa.Nombre) + "</span>";

            chkProgramas.Items.Add(new ListItem(texto, programa.IdPrograma.ToString()));
        }
    }

    private void MarcarProgramas(List<Programa> programasDelPaquete)
    {
        List<int> idsDelPaquete = new List<int>();

        foreach (Programa programa in programasDelPaquete)
        {
            idsDelPaquete.Add(programa.IdPrograma);
        }

        foreach (ListItem item in chkProgramas.Items)
        {
            item.Selected = idsDelPaquete.Contains(Convert.ToInt32(item.Value));
        }
    }

    private TipoPermiso ObtenerTipoPermisoSeleccionado()
    {
        return (TipoPermiso)Enum.Parse(typeof(TipoPermiso), ddlTipoPermiso.SelectedValue);
    }

    private int[] ObtenerProgramasSeleccionados()
    {
        List<int> idsPrograma = new List<int>();

        foreach (ListItem item in chkProgramas.Items)
        {
            if (item.Selected)
            {
                idsPrograma.Add(Convert.ToInt32(item.Value));
            }
        }

        return idsPrograma.ToArray();
    }

    private void CargarGrilla()
    {
        string filtro = txtFiltro.Text.Trim();

        List<Paquete> paquetes = paqueteService.ListarPaquetes(filtro, ucPaginador.NumeroPagina, tamanioPagina);
        int total = paqueteService.ContarPaquetes(filtro);

        gvPaquetes.DataSource = paquetes;
        gvPaquetes.DataBind();

        ucPaginador.Actualizar(total, paquetes.Count);
    }
}
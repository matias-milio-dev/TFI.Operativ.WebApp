using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class PermisosFamilia : PaginaSeguraBase
{
    private readonly IFamiliaService familiaService;
    private readonly IPatenteService patenteService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.GestionarFamilias }; }
    }

    public PermisosFamilia()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        familiaService = fabricaSeguridad.CrearFamiliaService();
        patenteService = fabricaSeguridad.CrearPatenteService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        lnkVolver.NavigateUrl = "~/Paginas/Usuarios/GestionUsuarios.aspx";
        txtBuscarPermiso.Attributes["placeholder"] = TextoRecurso.Obtener("EtiquetaBuscarPermiso");

        if (!IsPostBack)
        {
            CargarFamilias();
            CargarPagina();
        }
    }

    protected void ddlFamilia_SelectedIndexChanged(object sender, EventArgs e)
    {
        CargarPagina();
    }

    protected void rptCategorias_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
        {
            return;
        }

        GrupoPermisos grupo = (GrupoPermisos)e.Item.DataItem;

        HtmlGenericControl contenedorGrupo = (HtmlGenericControl)e.Item.FindControl("grupoPermiso");
        contenedorGrupo.Attributes["data-categoria"] = grupo.Clave;

        if (grupo.ExpandidoPorDefecto)
        {
            contenedorGrupo.Attributes["class"] = contenedorGrupo.Attributes["class"] + " expandido";
        }

        HtmlGenericControl spanTitulo = (HtmlGenericControl)e.Item.FindControl("spanTituloCategoria");
        spanTitulo.InnerText = grupo.Titulo;

        string formatoSeleccionados = TextoRecurso.Obtener("EtiquetaSeleccionadosCategoria");
        string formatoSeleccionadoSingular = TextoRecurso.Obtener("EtiquetaSeleccionadoCategoriaSingular");
        HtmlGenericControl spanSeleccionados = (HtmlGenericControl)e.Item.FindControl("spanSeleccionadosCategoria");
        spanSeleccionados.InnerText = string.Format(
            grupo.CantidadSeleccionada == 1 ? formatoSeleccionadoSingular : formatoSeleccionados,
            grupo.CantidadSeleccionada);
        spanSeleccionados.Attributes["data-formato"] = formatoSeleccionados;
        spanSeleccionados.Attributes["data-formato-singular"] = formatoSeleccionadoSingular;

        if (grupo.CantidadSeleccionada > 0)
        {
            HtmlButton btnAlternar = (HtmlButton)e.Item.FindControl("btnAlternarCategoria");
            btnAlternar.Attributes["class"] = btnAlternar.Attributes["class"] + " grupo-permiso-alternar-activo";
        }

        Repeater rptPermisos = (Repeater)e.Item.FindControl("rptPermisos");
        rptPermisos.DataSource = grupo.Permisos;
        rptPermisos.DataBind();
    }

    protected void rptPermisos_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
        {
            return;
        }

        ItemPermiso permiso = (ItemPermiso)e.Item.DataItem;

        HtmlGenericControl fila = (HtmlGenericControl)e.Item.FindControl("filaPermiso");
        fila.Attributes["data-nombre"] = permiso.Nombre.ToLowerInvariant();
        fila.Attributes["data-descripcion"] = permiso.Descripcion.ToLowerInvariant();

        HiddenField hidIdPatente = (HiddenField)e.Item.FindControl("hidIdPatente");
        hidIdPatente.Value = permiso.IdPatente.ToString();

        CheckBox chk = (CheckBox)e.Item.FindControl("chkSeleccionada");
        chk.Checked = permiso.Seleccionada;
        chk.Enabled = permiso.Habilitada;

        HtmlGenericControl spanNombre = (HtmlGenericControl)e.Item.FindControl("spanNombrePatente");
        spanNombre.InnerText = permiso.Nombre;

        HtmlGenericControl spanDescripcion = (HtmlGenericControl)e.Item.FindControl("spanDescripcionPatente");
        spanDescripcion.InnerText = permiso.Descripcion;

        if (!permiso.Habilitada)
        {
            HtmlGenericControl spanBadge = (HtmlGenericControl)e.Item.FindControl("spanBadgeProtegida");
            spanBadge.InnerText = TextoRecurso.Obtener("EtiquetaPermisoProtegido");
            spanBadge.Attributes["class"] = "badge-permiso badge-permiso-heredado";
            spanBadge.Visible = true;
        }
    }

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            int idFamilia = ObtenerIdFamiliaSeleccionada();
            List<int> idsActuales = ObtenerIds(familiaService.GetPatentesDeFamilia(idFamilia));
            List<int> idsAAsignar = new List<int>();
            List<int> idsAQuitar = new List<int>();

            foreach (RepeaterItem itemCategoria in rptCategorias.Items)
            {
                Repeater rptPermisos = (Repeater)itemCategoria.FindControl("rptPermisos");

                foreach (RepeaterItem itemPermiso in rptPermisos.Items)
                {
                    ClasificarPermisoDeFila(itemPermiso, idsActuales, idsAAsignar, idsAQuitar);
                }
            }

            if (!ValidarPatente(NombrePatente.GestionarFamilias))
            {
                return;
            }

            familiaService.AsignarPatentes(idFamilia, idsAAsignar.ToArray());
            familiaService.QuitarPatentes(idFamilia, idsAQuitar.ToArray());

            ControlNotificaciones.MostrarExito("MensajeExitoPermisosFamilia");
            CargarPagina();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void ClasificarPermisoDeFila(RepeaterItem itemPermiso, List<int> idsActuales, List<int> idsAAsignar, List<int> idsAQuitar)
    {
        CheckBox chk = (CheckBox)itemPermiso.FindControl("chkSeleccionada");

        if (!chk.Enabled)
        {
            return;
        }

        HiddenField hidIdPatente = (HiddenField)itemPermiso.FindControl("hidIdPatente");
        int idPatente = Convert.ToInt32(hidIdPatente.Value);
        bool yaAsignada = idsActuales.Contains(idPatente);

        if (chk.Checked && !yaAsignada)
        {
            idsAAsignar.Add(idPatente);
        }
        else if (!chk.Checked && yaAsignada)
        {
            idsAQuitar.Add(idPatente);
        }
    }

    private void CargarFamilias()
    {
        ddlFamilia.DataSource = familiaService.ListarFamilias();
        ddlFamilia.DataTextField = "Nombre";
        ddlFamilia.DataValueField = "IdFamilia";
        ddlFamilia.DataBind();

        string idSolicitado = Request.QueryString["idFamilia"];

        if (!string.IsNullOrEmpty(idSolicitado) && ddlFamilia.Items.FindByValue(idSolicitado) != null)
        {
            ddlFamilia.SelectedValue = idSolicitado;
        }
    }

    private int ObtenerIdFamiliaSeleccionada()
    {
        return Convert.ToInt32(ddlFamilia.SelectedValue);
    }

    private void CargarPagina()
    {
        Familia familia = familiaService.GetFamiliaPorId(ObtenerIdFamiliaSeleccionada());

        tituloPermisos.InnerText = TextoRecurso.Formato("TituloPermisosFamilia", familia.Nombre);

        rptCategorias.DataSource = ArmarGrupos(familia);
        rptCategorias.DataBind();
    }

    private List<GrupoPermisos> ArmarGrupos(Familia familia)
    {
        List<int> idsAsignados = ObtenerIds(familiaService.GetPatentesDeFamilia(familia.IdFamilia));
        List<Patente> todasLasPatentes = patenteService.ListarTodas();

        List<GrupoPermisos> grupos = new List<GrupoPermisos>();

        foreach (CategoriaPatente categoria in CategoriaPatente.ObtenerTodas())
        {
            GrupoPermisos grupo = new GrupoPermisos
            {
                Clave = categoria.Tipo.ToString(),
                Titulo = TextoRecurso.Obtener(categoria.ClaveRecurso),
                Permisos = new List<ItemPermiso>()
            };

            foreach (Patente patente in todasLasPatentes)
            {
                if (CategoriaPatente.ObtenerPorPatente(patente.Nombre) != categoria)
                {
                    continue;
                }

                ItemPermiso permiso = new ItemPermiso
                {
                    IdPatente = patente.IdPatente,
                    Nombre = patente.Nombre,
                    Descripcion = patente.Descripcion,
                    Seleccionada = idsAsignados.Contains(patente.IdPatente),
                    Habilitada = !EsPermisoProtegido(familia, patente)
                };

                grupo.Permisos.Add(permiso);

                if (permiso.Seleccionada)
                {
                    grupo.CantidadSeleccionada++;
                }
            }

            if (grupo.Permisos.Count > 0)
            {
                grupo.ExpandidoPorDefecto = grupos.Count == 0;
                grupos.Add(grupo);
            }
        }

        return grupos;
    }

    // El Administrador no puede quitarse a si mismo la gestion de familias: nadie mas podria devolversela.
    private bool EsPermisoProtegido(Familia familia, Patente patente)
    {
        return familia.Nombre == NombreFamilia.Administrador && patente.Nombre == NombrePatente.GestionarFamilias;
    }

    private List<int> ObtenerIds(List<Patente> patentes)
    {
        List<int> ids = new List<int>();

        foreach (Patente patente in patentes)
        {
            ids.Add(patente.IdPatente);
        }

        return ids;
    }

    private class GrupoPermisos
    {
        public string Clave;
        public string Titulo;
        public bool ExpandidoPorDefecto;
        public int CantidadSeleccionada;
        public List<ItemPermiso> Permisos;
    }

    private class ItemPermiso
    {
        public int IdPatente;
        public string Nombre;
        public string Descripcion;
        public bool Seleccionada;
        public bool Habilitada;
    }
}

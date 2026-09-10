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
public partial class PermisosUsuario : PaginaSeguraBase
{
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;
    private readonly IPatenteService patenteService;
    private int idUsuario;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.AsignarPatente, NombrePatente.RemoverPatente }; }
    }

    public PermisosUsuario()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        familiaService = fabricaSeguridad.CrearFamiliaService();
        patenteService = fabricaSeguridad.CrearPatenteService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        idUsuario = Convert.ToInt32(Request.QueryString["idUsuario"]);

        lnkVolver.NavigateUrl = "~/Paginas/Usuarios/GestionUsuarios.aspx";
        txtBuscarPermiso.Attributes["placeholder"] = TextoRecurso.Obtener("EtiquetaBuscarPermiso");

        if (!IsPostBack)
        {
            CargarFiltroFamilia();
            CargarPagina();
        }
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
        fila.Attributes["data-familias"] = permiso.IdsFamilias;

        HiddenField hidIdPatente = (HiddenField)e.Item.FindControl("hidIdPatente");
        hidIdPatente.Value = permiso.IdPatente.ToString();

        CheckBox chk = (CheckBox)e.Item.FindControl("chkSeleccionada");
        chk.Checked = permiso.Seleccionada;
        chk.Enabled = permiso.Habilitada;

        HtmlGenericControl spanNombre = (HtmlGenericControl)e.Item.FindControl("spanNombrePatente");
        spanNombre.InnerText = permiso.Nombre;

        HtmlGenericControl spanDescripcion = (HtmlGenericControl)e.Item.FindControl("spanDescripcionPatente");
        spanDescripcion.InnerText = permiso.Descripcion;

        HtmlGenericControl spanBadge = (HtmlGenericControl)e.Item.FindControl("spanBadgeHeredada");
        spanBadge.InnerText = TextoRecurso.Obtener(permiso.HeredadaPorFamilia ? "EtiquetaHeredadaPorFamilia" : "EtiquetaNoHeredada");
        spanBadge.Attributes["class"] = permiso.HeredadaPorFamilia
            ? "badge-permiso badge-permiso-heredado"
            : "badge-permiso badge-permiso-no-heredado";
    }

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            List<int> idsIndividuales = ObtenerIds(patenteService.GetPatentesIndividualesDeUsuario(idUsuario));
            List<int> idsAAsignar = new List<int>();
            List<int> idsAQuitar = new List<int>();

            foreach (RepeaterItem itemCategoria in rptCategorias.Items)
            {
                Repeater rptPermisos = (Repeater)itemCategoria.FindControl("rptPermisos");

                foreach (RepeaterItem itemPermiso in rptPermisos.Items)
                {
                    ClasificarPermisoDeFila(itemPermiso, idsIndividuales, idsAAsignar, idsAQuitar);
                }
            }

            if (idsAAsignar.Count > 0 && !ValidarPatente(NombrePatente.AsignarPatente))
            {
                return;
            }

            if (idsAQuitar.Count > 0 && !ValidarPatente(NombrePatente.RemoverPatente))
            {
                return;
            }

            patenteService.AsignarPatentes(idUsuario, idsAAsignar.ToArray());
            patenteService.QuitarPatentes(idUsuario, idsAQuitar.ToArray());

            ControlNotificaciones.MostrarExito("MensajeExitoPermisosUsuario");
            CargarPagina();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void ClasificarPermisoDeFila(RepeaterItem itemPermiso, List<int> idsIndividuales, List<int> idsAAsignar, List<int> idsAQuitar)
    {
        CheckBox chk = (CheckBox)itemPermiso.FindControl("chkSeleccionada");

        if (!chk.Enabled)
        {
            return;
        }

        HiddenField hidIdPatente = (HiddenField)itemPermiso.FindControl("hidIdPatente");
        int idPatente = Convert.ToInt32(hidIdPatente.Value);
        bool yaAsignada = idsIndividuales.Contains(idPatente);

        if (chk.Checked && !yaAsignada)
        {
            idsAAsignar.Add(idPatente);
        }
        else if (!chk.Checked && yaAsignada)
        {
            idsAQuitar.Add(idPatente);
        }
    }

    private void CargarFiltroFamilia()
    {
        ddlFiltroFamilia.DataSource = familiaService.ListarFamilias();
        ddlFiltroFamilia.DataTextField = "Nombre";
        ddlFiltroFamilia.DataValueField = "IdFamilia";
        ddlFiltroFamilia.DataBind();

        ddlFiltroFamilia.Items.Insert(0, new ListItem(TextoRecurso.Obtener("EtiquetaFiltrarPorFamilia"), string.Empty));
    }

    private void CargarPagina()
    {
        Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);

        tituloPermisos.InnerText = TextoRecurso.Formato("TituloPermisosUsuario", usuario.NombreUsuario);

        rptCategorias.DataSource = ArmarGrupos(usuario);
        rptCategorias.DataBind();
    }

    private List<GrupoPermisos> ArmarGrupos(Usuario usuario)
    {
        List<int> idsPatentesFamilia = ObtenerIdsPatentesFamilia(usuario);
        List<int> idsIndividuales = ObtenerIds(patenteService.GetPatentesIndividualesDeUsuario(idUsuario));
        List<Patente> todasLasPatentes = patenteService.ListarTodas();
        Dictionary<int, List<int>> familiasPorPatente = ObtenerFamiliasPorPatente();

        bool puedeAsignar = AutorizacionHandler.TienePatente(NombrePatente.AsignarPatente);
        bool puedeRemover = AutorizacionHandler.TienePatente(NombrePatente.RemoverPatente);

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
                    HeredadaPorFamilia = idsPatentesFamilia.Contains(patente.IdPatente),
                    Seleccionada = idsIndividuales.Contains(patente.IdPatente),
                    IdsFamilias = ObtenerIdsFamiliasComoTexto(familiasPorPatente, patente.IdPatente)
                };
                permiso.Habilitada = permiso.Seleccionada ? puedeRemover : puedeAsignar;

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

    private Dictionary<int, List<int>> ObtenerFamiliasPorPatente()
    {
        Dictionary<int, List<int>> resultado = new Dictionary<int, List<int>>();

        foreach (Familia familia in familiaService.ListarFamilias())
        {
            foreach (Patente patente in familiaService.GetPatentesDeFamilia(familia.IdFamilia))
            {
                if (!resultado.ContainsKey(patente.IdPatente))
                {
                    resultado[patente.IdPatente] = new List<int>();
                }

                resultado[patente.IdPatente].Add(familia.IdFamilia);
            }
        }

        return resultado;
    }

    private string ObtenerIdsFamiliasComoTexto(Dictionary<int, List<int>> familiasPorPatente, int idPatente)
    {
        if (!familiasPorPatente.ContainsKey(idPatente))
        {
            return string.Empty;
        }

        return string.Join(",", familiasPorPatente[idPatente]);
    }

    private List<int> ObtenerIdsPatentesFamilia(Usuario usuario)
    {
        if (usuario.Familias.Count == 0)
        {
            return new List<int>();
        }

        return ObtenerIds(familiaService.GetPatentesDeFamilia(usuario.Familias[0].IdFamilia));
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
        public bool HeredadaPorFamilia;
        public bool Seleccionada;
        public bool Habilitada;
        public string IdsFamilias;
    }
}

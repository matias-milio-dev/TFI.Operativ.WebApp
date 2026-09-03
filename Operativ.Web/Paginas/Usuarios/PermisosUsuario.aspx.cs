using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class PermisosUsuario : PaginaSeguraBase
{
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;
    private readonly IPatenteService patenteService;
    private int idUsuario;

    protected override string[] PerfilesPermitidos
    {
        get { return new[] { NavegacionHelper.PerfilAdministrador }; }
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

        bool puedeAsignar = AutorizacionHandler.TienePatente(NombrePatente.AsignarPatente);
        bool puedeRemover = AutorizacionHandler.TienePatente(NombrePatente.RemoverPatente);

        if (!puedeAsignar && !puedeRemover)
        {
            Response.Redirect("~/Paginas/Comun/NoAutorizado.aspx");
            return;
        }

        lnkVolver.NavigateUrl = "~/Paginas/Usuarios/GestionUsuarios.aspx";
        txtBuscarPermiso.Attributes["placeholder"] = (string)GetGlobalResourceObject("Textos", "EtiquetaBuscarPermiso");

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

        string formatoSeleccionados = (string)GetGlobalResourceObject("Textos", "EtiquetaSeleccionadosCategoria");
        string formatoSeleccionadoSingular = (string)GetGlobalResourceObject("Textos", "EtiquetaSeleccionadoCategoriaSingular");
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

        if (permiso.HeredadaPorFamilia)
        {
            spanBadge.InnerText = (string)GetGlobalResourceObject("Textos", "EtiquetaHeredadaPorFamilia");
            spanBadge.Attributes["class"] = "badge-permiso badge-permiso-heredado";
        }
        else
        {
            spanBadge.InnerText = (string)GetGlobalResourceObject("Textos", "EtiquetaNoHeredada");
            spanBadge.Attributes["class"] = "badge-permiso badge-permiso-no-heredado";
        }
    }

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            List<Patente> patentesIndividuales = patenteService.GetPatentesIndividualesDeUsuario(idUsuario);

            foreach (RepeaterItem itemCategoria in rptCategorias.Items)
            {
                Repeater rptPermisos = (Repeater)itemCategoria.FindControl("rptPermisos");

                foreach (RepeaterItem itemPermiso in rptPermisos.Items)
                {
                    GuardarPermisoDeFila(itemPermiso, patentesIndividuales);
                }
            }

            ControlNotificaciones.MostrarExito("MensajeExitoPermisosUsuario");
            CargarPagina();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void GuardarPermisoDeFila(RepeaterItem itemPermiso, List<Patente> patentesIndividuales)
    {
        CheckBox chk = (CheckBox)itemPermiso.FindControl("chkSeleccionada");

        if (!chk.Enabled)
        {
            return;
        }

        HiddenField hidIdPatente = (HiddenField)itemPermiso.FindControl("hidIdPatente");
        int idPatente = Convert.ToInt32(hidIdPatente.Value);
        bool yaAsignada = TienePatenteIndividual(patentesIndividuales, idPatente);

        if (chk.Checked && !yaAsignada)
        {
            patenteService.AsignarPatente(idUsuario, idPatente);
        }
        else if (!chk.Checked && yaAsignada)
        {
            patenteService.QuitarPatente(idUsuario, idPatente);
        }
    }

    private void CargarFiltroFamilia()
    {
        List<Familia> familias = familiaService.ListarFamilias();

        ddlFiltroFamilia.DataSource = familias;
        ddlFiltroFamilia.DataTextField = "Nombre";
        ddlFiltroFamilia.DataValueField = "IdFamilia";
        ddlFiltroFamilia.DataBind();

        string textoTodas = (string)GetGlobalResourceObject("Textos", "EtiquetaFiltrarPorFamilia");
        ddlFiltroFamilia.Items.Insert(0, new ListItem(textoTodas, string.Empty));
    }

    private void CargarPagina()
    {
        Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);

        string formatoTitulo = (string)GetGlobalResourceObject("Textos", "TituloPermisosUsuario");
        tituloPermisos.InnerText = string.Format(formatoTitulo, usuario.NombreUsuario);

        rptCategorias.DataSource = ArmarGrupos(usuario);
        rptCategorias.DataBind();
    }

    private List<GrupoPermisos> ArmarGrupos(Usuario usuario)
    {
        List<int> idsPatentesFamilia = ObtenerIdsPatentesFamilia(usuario);
        List<Patente> patentesIndividuales = patenteService.GetPatentesIndividualesDeUsuario(idUsuario);
        List<Patente> todasLasPatentes = patenteService.ListarTodas();
        Dictionary<int, List<int>> familiasPorPatente = ObtenerFamiliasPorPatente();

        bool puedeAsignar = AutorizacionHandler.TienePatente(NombrePatente.AsignarPatente);
        bool puedeRemover = AutorizacionHandler.TienePatente(NombrePatente.RemoverPatente);

        List<GrupoPermisos> grupos = new List<GrupoPermisos>();

        foreach (string categoria in CategoriaPatente.Orden)
        {
            GrupoPermisos grupo = new GrupoPermisos();
            grupo.Clave = categoria;
            grupo.Titulo = (string)GetGlobalResourceObject("Textos", "Categoria" + categoria);
            grupo.Permisos = new List<ItemPermiso>();

            foreach (Patente patente in todasLasPatentes)
            {
                if (CategoriaPatente.Obtener(patente.Nombre) != categoria)
                {
                    continue;
                }

                ItemPermiso permiso = new ItemPermiso();
                permiso.IdPatente = patente.IdPatente;
                permiso.Nombre = patente.Nombre;
                permiso.Descripcion = patente.Descripcion;
                permiso.HeredadaPorFamilia = idsPatentesFamilia.Contains(patente.IdPatente);
                permiso.Seleccionada = TienePatenteIndividual(patentesIndividuales, patente.IdPatente);
                permiso.Habilitada = permiso.Seleccionada ? puedeRemover : puedeAsignar;
                permiso.IdsFamilias = ObtenerIdsFamiliasComoTexto(familiasPorPatente, patente.IdPatente);

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
            List<Patente> patentesDeFamilia = familiaService.GetPatentesDeFamilia(familia.IdFamilia);

            foreach (Patente patente in patentesDeFamilia)
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

        List<string> textos = new List<string>();

        foreach (int idFamilia in familiasPorPatente[idPatente])
        {
            textos.Add(idFamilia.ToString());
        }

        return string.Join(",", textos);
    }

    private List<int> ObtenerIdsPatentesFamilia(Usuario usuario)
    {
        List<int> ids = new List<int>();

        if (usuario.Familias.Count == 0)
        {
            return ids;
        }

        List<Patente> patentesFamilia = familiaService.GetPatentesDeFamilia(usuario.Familias[0].IdFamilia);

        foreach (Patente patente in patentesFamilia)
        {
            ids.Add(patente.IdPatente);
        }

        return ids;
    }

    private bool TienePatenteIndividual(List<Patente> patentes, int idPatente)
    {
        foreach (Patente patente in patentes)
        {
            if (patente.IdPatente == idPatente)
            {
                return true;
            }
        }

        return false;
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

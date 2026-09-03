using System;
using System.Collections.Generic;
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
        RegistrarTextosParaJavaScript();

        if (!IsPostBack)
        {
            CargarFiltroFamilia();
            CargarPagina();
        }
    }

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            List<Patente> patentesIndividuales = patenteService.GetPatentesIndividualesDeUsuario(idUsuario);

            foreach (ListItem item in chkPatentes.Items)
            {
                if (!item.Enabled)
                {
                    continue;
                }

                int idPatente = Convert.ToInt32(item.Value);
                bool yaAsignada = TienePatenteIndividual(patentesIndividuales, idPatente);

                if (item.Selected && !yaAsignada)
                {
                    patenteService.AsignarPatente(idUsuario, idPatente);
                }
                else if (!item.Selected && yaAsignada)
                {
                    patenteService.QuitarPatente(idUsuario, idPatente);
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

    private void RegistrarTextosParaJavaScript()
    {
        string script = "window.OperativTextosPermisos = {"
            + "heredada: \"" + EscaparParaJavaScript((string)GetGlobalResourceObject("Textos", "EtiquetaHeredadaPorFamilia")) + "\","
            + "noHeredada: \"" + EscaparParaJavaScript((string)GetGlobalResourceObject("Textos", "EtiquetaNoHeredada")) + "\","
            + "formatoCantidad: \"" + EscaparParaJavaScript((string)GetGlobalResourceObject("Textos", "EtiquetaCantidadPermisos")) + "\","
            + "formatoSeleccionados: \"" + EscaparParaJavaScript((string)GetGlobalResourceObject("Textos", "EtiquetaSeleccionadosCategoria")) + "\""
            + "};";

        ClientScript.RegisterStartupScript(GetType(), "TextosPermisos", script, true);
    }

    private static string EscaparParaJavaScript(string texto)
    {
        return texto.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private void CargarFiltroFamilia()
    {
        List<Familia> familias = familiaService.ListarFamilias();

        ddlFiltroFamilia.DataSource = familias;
        ddlFiltroFamilia.DataTextField = "Nombre";
        ddlFiltroFamilia.DataValueField = "IdFamilia";
        ddlFiltroFamilia.DataBind();

        string textoTodas = (string)GetGlobalResourceObject("Textos", "EtiquetaTodasLasFamilias");
        ddlFiltroFamilia.Items.Insert(0, new ListItem(textoTodas, string.Empty));
    }

    private void CargarPagina()
    {
        Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);

        string formatoTitulo = (string)GetGlobalResourceObject("Textos", "TituloPermisosUsuario");
        tituloPermisos.InnerText = string.Format(formatoTitulo, usuario.NombreUsuario);

        List<int> idsPatentesFamilia = ObtenerIdsPatentesFamilia(usuario);
        List<Patente> patentesIndividuales = patenteService.GetPatentesIndividualesDeUsuario(idUsuario);
        List<Patente> todasLasPatentes = patenteService.ListarTodas();
        Dictionary<int, List<int>> familiasPorPatente = ObtenerFamiliasPorPatente();

        bool puedeAsignar = AutorizacionHandler.TienePatente(NombrePatente.AsignarPatente);
        bool puedeRemover = AutorizacionHandler.TienePatente(NombrePatente.RemoverPatente);

        chkPatentes.Items.Clear();

        foreach (string categoria in CategoriaPatente.Orden)
        {
            string tituloCategoria = (string)GetGlobalResourceObject("Textos", "Categoria" + categoria);

            foreach (Patente patente in todasLasPatentes)
            {
                if (CategoriaPatente.Obtener(patente.Nombre) != categoria)
                {
                    continue;
                }

                bool heredada = idsPatentesFamilia.Contains(patente.IdPatente);
                bool seleccionada = TienePatenteIndividual(patentesIndividuales, patente.IdPatente);

                ListItem item = new ListItem(patente.Nombre, patente.IdPatente.ToString());
                item.Selected = seleccionada;
                item.Enabled = seleccionada ? puedeRemover : puedeAsignar;
                item.Attributes["class"] = "chk-permiso";
                item.Attributes["data-categoria"] = categoria;
                item.Attributes["data-categoria-titulo"] = tituloCategoria;
                item.Attributes["data-descripcion"] = patente.Descripcion;
                item.Attributes["data-heredada"] = heredada ? "1" : "0";
                item.Attributes["data-familias"] = ObtenerIdsFamiliasComoTexto(familiasPorPatente, patente.IdPatente);

                chkPatentes.Items.Add(item);
            }
        }
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
}

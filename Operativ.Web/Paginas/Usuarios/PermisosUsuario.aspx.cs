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

        if (!IsPostBack)
        {
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

    private void CargarPagina()
    {
        Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);

        string formato = (string)GetGlobalResourceObject("Textos", "TituloPermisosUsuario");
        tituloPermisos.InnerText = string.Format(formato, usuario.NombreUsuario);

        List<int> idsPatentesFamilia = ObtenerIdsPatentesFamilia(usuario);
        List<Patente> patentesIndividuales = patenteService.GetPatentesIndividualesDeUsuario(idUsuario);

        bool puedeAsignar = AutorizacionHandler.TienePatente(NombrePatente.AsignarPatente);
        bool puedeRemover = AutorizacionHandler.TienePatente(NombrePatente.RemoverPatente);

        string sufijoFamilia = (string)GetGlobalResourceObject("Textos", "EtiquetaYaPorFamilia");

        chkPatentes.Items.Clear();

        foreach (Patente patente in patenteService.ListarTodas())
        {
            string texto = patente.Nombre;

            if (idsPatentesFamilia.Contains(patente.IdPatente))
            {
                texto = texto + " " + sufijoFamilia;
            }

            ListItem item = new ListItem(texto, patente.IdPatente.ToString());
            item.Selected = TienePatenteIndividual(patentesIndividuales, patente.IdPatente);

            if (item.Selected && !puedeRemover)
            {
                item.Enabled = false;
            }
            else if (!item.Selected && !puedeAsignar)
            {
                item.Enabled = false;
            }

            chkPatentes.Items.Add(item);
        }
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
